using System.Text;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;

namespace Nuages.Fido2.Storage.EntityFramework;

public class Fido2StorageEntityFramework : IFido2Storage
{
    private readonly InMemoryFido2DbContext _context;
    private readonly IFido2UserStore _userStore;

    public Fido2StorageEntityFramework(InMemoryFido2DbContext context, IFido2UserStore userStore)
    {
        _context = context;
        _userStore = userStore;
    }

    public async Task<Fido2User?> GetUserAsync(string userName)
    {
        return await _userStore.GetUserAsync(userName);
    }

    public async Task<List<IFido2Credential>> GetCredentialsByUserAsync(Fido2User user)
    {
        var idBase64 = Convert.ToBase64String(user.Id);

        var res = await _context.Fido2Credentials.Where(c => c.UserIdBase64 == idBase64).ToListAsync();
        
        return res.Select(c => (IFido2Credential) c).ToList();
    }

    public Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] argsCredentialId, object cancellationToken)
    {
        return Task.FromResult(new List<Fido2User>());
    }

    public async Task AddCredentialToUserAsync(Fido2User user, IFido2Credential credential)
    {
        var newCredential = (Fido2Credential)credential;
        
        newCredential.UserId = user.Id;
        newCredential.UserIdBase64 = Convert.ToBase64String(user.Id);

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
}