using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Fido2.AspNetIdentity;

namespace Nuages.Identity.Services.Fido2.Storage;

public class Fido2StorageEntityFramework<T> : IFido2Storage
    where T : NuagesIdentityDbContext
{
    private readonly T _context;
    private readonly IFido2UserStore _userStore;

    public Fido2StorageEntityFramework(T context, IFido2UserStore userStore)
    {
        _context = context;
        _userStore = userStore;
    }

    public async Task<Fido2User?> GetUserByUsernameAsync(string userName)
    {
        return await _userStore.GetUserByUsernameAsync(userName);
    }
    
    public async Task<string?> GetUserEmailAsync(byte[] id)
    {
        return await _userStore.GetUserEmailAsync(id);
    }

    public async Task<List<IFido2Credential>> GetCredentialsByUserAsync(Fido2User user)
    {
        var idBase64 = Convert.ToBase64String(user.Id);

        var res = await _context.Fido2Credentials.Where(c => c.UserIdBase64 == idBase64).ToListAsync();
        
        return res.Select(c => (IFido2Credential) c).ToList();
    }

    public Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] credentialId)
    {
        var idBase64 = Convert.ToBase64String(credentialId);
        
        var creds = _context.Fido2Credentials
            .Where(c => c.DescriptorIdBase64 == idBase64);

        return Task.FromResult(creds.Select(c => new Fido2User
        {
            DisplayName = c.DisplayName,
            Name = c.DisplayName,
            Id = c.UserId
        }).ToList());
    }

    public async Task AddCredentialToUserAsync(Fido2User user, IFido2Credential credential)
    {
        var newCredential = (Fido2Credential)credential;
        
        newCredential.UserId = user.Id;
        newCredential.UserIdBase64 = Convert.ToBase64String(user.Id);
        newCredential.UserHandleBase64 = Convert.ToBase64String(credential.UserHandle);
        newCredential.DisplayName = user.DisplayName;
        
        await _context.Fido2Credentials.AddAsync(newCredential);
        await _context.SaveChangesAsync();
    }

    public void Initialize()
    {
        
    }

    public IFido2Credential CreateCredential(PublicKeyCredentialDescriptor publicKeyCredentialDescriptor, byte[] resultPublicKey,
        byte[] userId, uint resultCounter, string credType, DateTime regDate, Guid aaguid)
    {
        return new Fido2Credential
        {
            Id = Guid.NewGuid().ToString(),
            Descriptor = publicKeyCredentialDescriptor,
            PublicKey = resultPublicKey,
            UserHandle = userId,
            SignatureCounter = resultCounter,
            CredType = credType,
            RegDate = regDate,
            AaGuid = aaguid
        };
    }
    
    public Task<IFido2Credential?> GetCredentialByIdAsync(byte[] id)
    {
        var base64Id = Convert.ToBase64String(id);
        
        return Task.FromResult((IFido2Credential?)_context.Fido2Credentials
            .FirstOrDefault(c => c.DescriptorIdBase64 == base64Id));
    }

    public Task<List<IFido2Credential>> GetCredentialsByUserHandleAsync(byte[] userHandle)
    {
        var base64Handle = Convert.ToBase64String(userHandle);
        
        return Task.FromResult(_context.Fido2Credentials.Where(c => c.UserHandleBase64 == base64Handle).ToList().Select(c => (IFido2Credential) c).ToList());
    }

    public async Task UpdateCounterAsync(byte[] credentialId, uint counter)
    {
        var cred = await GetCredentialByIdAsync(credentialId);

        if (cred != null)
        {
            cred.SignatureCounter = counter;

            await _context.SaveChangesAsync();
        }
    }
    
    public async Task RemoveCredentialFromUser(byte[] userId, byte[] keyId)
    {
        var key = await GetCredentialByIdAsync(keyId);

        if (key != null)
        {
            if (key.UserId.SequenceEqual(userId))
            {
                _context.Fido2Credentials.Remove((Fido2Credential) key);
                await _context.SaveChangesAsync();
            }
        }
       
    }
}