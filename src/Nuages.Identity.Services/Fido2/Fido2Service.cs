using Fido2NetLib;
using Fido2NetLib.Objects;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Identity.Services.Fido2.Models;
using Nuages.Identity.Services.Fido2.Storage;

namespace Nuages.Identity.Services.Fido2;

public class Fido2Service : IFido2Service
{
    private readonly IFido2 _fido2;
    private readonly IFido2Storage _fido2Storage;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IMessageService _messageService;
    private readonly IIdentityEventBus _identityEventBus;

    public Fido2Service(IFido2 fido2, IFido2Storage fido2Storage, IHttpContextAccessor contextAccessor, 
                        IMessageService messageService, IIdentityEventBus identityEventBus)
    {
        _fido2 = fido2;
        
        _fido2Storage = fido2Storage;
        _fido2Storage.Initialize();
        
        _contextAccessor = contextAccessor;
        _messageService = messageService;
        _identityEventBus = identityEventBus;
    }

    public async Task<CredentialCreateOptions> MakeCredentialOptionsAsync(MakeCredentialOptionsRequest request)
    {
        try
        {
            // 1. Get user from DB by username (in our example, auto create missing users)
            var user = await _fido2Storage.GetUserByUsernameAsync(request.UserName);
            if (user == null)
                throw new Exception("NotFound");

            user.DisplayName = request.DisplayName;
            // 2. Get user existing keys by username
            var existingKeys = (await _fido2Storage.GetCredentialsByUserAsync(user)).Select(c => c.Descriptor).ToList();

            // 3. Create options
            var authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = request.RequireResidentKey,
                UserVerification = request.UserVerification,
                AuthenticatorAttachment = request.AuthType
            };

            var exts = new AuthenticationExtensionsClientInputs
            {
                Extensions = true,
                UserVerificationMethod = true
            };

            var options = _fido2.RequestNewCredential(user, existingKeys, authenticatorSelection,
                request.AttestationType, exts);

            // 4. Temporarily store options, session/in-memory cache/redis/db
            _contextAccessor.HttpContext!.Session.SetString("fido2.attestationOptions", options.ToJson());

            // 5. return options to client
            return options;
        }
        catch (Exception e)
        {
            return new CredentialCreateOptions { Status = "error", ErrorMessage = e.Message };
        }
    }

    public async Task<Fido2NetLib.Fido2.CredentialMakeResult> MakeNewCredentialAsync(AuthenticatorAttestationRawResponse attestationResponse)
    {
        var jsonOptions = _contextAccessor.HttpContext!.Session.GetString("fido2.attestationOptions");
        var options = CredentialCreateOptions.FromJson(jsonOptions);

             
         // Func<IsCredentialIdUniqueToUserParams, Task<bool>> callback = async (IsCredentialIdUniqueToUserParams args) =>
         // {
         //     var users = await _fido2Storage.GetUsersByCredentialIdAsync(args.CredentialId);
         //     if (users.Count > 0) return false;
         //
         //     return true;
         // };

         async Task<bool> Callback(IsCredentialIdUniqueToUserParams args, CancellationToken token)
         {
             var users = await _fido2Storage.GetUsersByCredentialIdAsync(args.CredentialId);
             return users.Count <= 0;
         }

         var success = await _fido2.MakeNewCredentialAsync(attestationResponse, options,  Callback);

         if (success == null)
             throw new Exception("success is null");
         
         if (success.Result == null)
             throw new Exception("success.Result is null");
         
        var credential = _fido2Storage.CreateCredential(new PublicKeyCredentialDescriptor(success.Result.CredentialId), 
            success.Result.PublicKey,
            success.Result.User.Id,
            success.Result.Counter,
            success.Result.CredType,
            DateTime.Now,
            success.Result.Aaguid);

        await _fido2Storage.AddCredentialToUserAsync(options.User, credential);
        
        
        await _identityEventBus.PutEvent(IdentityEvents.Fido2CredentialAdded, options.User);
        
        var email = await _fido2Storage.GetUserEmailAsync(options.User.Id);
        if (email != null)
        {
            _messageService.SendEmailUsingTemplate(email, "SecurityKey_Added", new Dictionary<string, string>());
        }
        return success;
    }

    public async Task<AssertionOptions> AssertionOptionAsync(AssertionOptionsRequest request)
    {
        var user = await _fido2Storage.GetUserByUsernameAsync(request.UserName) ?? throw new ArgumentException("Username was not registered");

        var existingCredentials = (await _fido2Storage.GetCredentialsByUserAsync(user)).ToList().Select(c => c.Descriptor).ToList();
        
        var exts = new AuthenticationExtensionsClientInputs
        { 
            UserVerificationMethod = true 
        };
        
        var options = _fido2.GetAssertionOptions(
            existingCredentials,
            request.UserVerification ?? UserVerificationRequirement.Preferred,
            exts
        );

        _contextAccessor.HttpContext!.Session.SetString("fido2.assertionOptions", options.ToJson());

       return options;
    }

    public async Task<AssertionVerificationResult> MakeAssertionAsync(AuthenticatorAssertionRawResponse clientResponse)
    {
        // 1. Get the assertion options we sent the client
        var jsonOptions = _contextAccessor.HttpContext!.Session.GetString("fido2.assertionOptions");
        var options = AssertionOptions.FromJson(jsonOptions);

        // 2. Get registered credential from database
        var creds = await _fido2Storage.GetCredentialByIdAsync(clientResponse.Id) ?? throw new Exception("Unknown credentials");

        // 3. Get credential counter from database
        var storedCounter = creds.SignatureCounter;

        // async Task<bool> Callback(IsUserHandleOwnerOfCredentialIdParams args)
        // {
        //     var storedCreds = await _fido2Storage.GetCredentialsByUserHandleAsync(args.UserHandle);
        //     return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
        // }

        async Task<bool> Callback(IsUserHandleOwnerOfCredentialIdParams args, CancellationToken token)
        {
            var storedCreds = await _fido2Storage.GetCredentialsByUserHandleAsync(args.UserHandle);
            return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
        }

        // 5. Make the assertion
        var res = await _fido2.MakeAssertionAsync(clientResponse, options, creds.PublicKey, storedCounter, Callback);

        // 6. Store the updated counter
        await _fido2Storage.UpdateCounterAsync(res.CredentialId, res.Counter);

        return res;
    }

    public async Task<List<IFido2Credential>> GetSecurityKeysForUser(byte[] userId)
    {
        var user = new Fido2User { Id = userId };
        return await _fido2Storage.GetCredentialsByUserAsync(user);
    }

    public async Task<bool> HasSecurityKeys(byte[] userId)
    {
        return (await GetSecurityKeysForUser(userId)).Any();
    }

    public  async Task RemoveKeyAsync(byte[] userId, byte[]  keyId)
    {
        await _fido2Storage.RemoveCredentialFromUser(userId, keyId);

        var email = await _fido2Storage.GetUserEmailAsync(userId);
        if (email != null)
        {
            _messageService.SendEmailUsingTemplate(email, "SecurityKey_Removed", new Dictionary<string, string>());
        }
       
        await _identityEventBus.PutEvent(IdentityEvents.Fido2CredentialRemoved, new { Email = userId});
    }
}