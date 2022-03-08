using Fido2NetLib;
using Fido2NetLib.Objects;
using Nuages.Fido2.Models;

namespace Nuages.Fido2;

public class Fido2Service : IFido2Service
{
    private readonly IFido2 _fido2;
    private readonly IFido2Storage _fido2Storage;
    private readonly IHttpContextAccessor _contextAccessor;

    public Fido2Service(IFido2 fido2, IFido2Storage fido2Storage, IHttpContextAccessor contextAccessor)
    {
        _fido2 = fido2;
        _fido2Storage = fido2Storage;
        _contextAccessor = contextAccessor;
    }

    public async Task<CredentialCreateOptions> MakeCredentialOptionsAsync(MakeCredentialOptionsRequest request)
    {
        try
        {
            // 1. Get user from DB by username (in our example, auto create missing users)
            var user = await _fido2Storage.GetUserAsync(request.UserName);
            if (user == null)
                throw new Exception("NotFound");
            
            // 2. Get user existing keys by username
            var existingKeys = (await _fido2Storage.GetCredentialsByUserAsync(user)).Select(c => c.Descriptor).ToList();

            // 3. Create options
            var authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = request.RequireResidentKey,
                UserVerification = request.UserVerification
            };

            authenticatorSelection.AuthenticatorAttachment = request.AuthType;

            var exts = new AuthenticationExtensionsClientInputs()
            {
                Extensions = true,
                UserVerificationMethod = true,
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

    public Task RegisterNewCredentialAsync(RegisterCredentialRequest request)
    {
        throw new NotImplementedException();
    }
}