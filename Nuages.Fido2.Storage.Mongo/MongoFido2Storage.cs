
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Nuages.Fido2.Storage.Mongo;


public class MongoFido2Storage : IFido2Storage
{
    private readonly IFido2UserStore _userStore;
    private readonly Fido2MongoOptions _options;
    
    private IMongoCollection<Fido2Credential> _credentialCollection = null!;

    public MongoFido2Storage(IOptions<Fido2MongoOptions> options, IFido2UserStore userStore)
    {
        _userStore = userStore;
        _options = options.Value;
    }
    
    public async Task<Fido2User?> GetUserAsync(string userName)
    {
        return await _userStore.GetUserAsync(userName);
    }

    public Task<List<IFido2Credential>> GetCredentialsByUserAsync(Fido2User user)
    {
        var idBase64 = Convert.ToBase64String(user.Id);

        var res = _credentialCollection.AsQueryable().Where(c => c.UserIdBase64 == idBase64).ToList();
        
        return Task.FromResult(res.Select(c => (IFido2Credential) c).ToList());
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
        newCredential.DescriptorIdBase64 = Convert.ToBase64String(credential.Descriptor.Id);
        newCredential.UserHandleBase64 = Convert.ToBase64String(credential.UserHandle);
        
        await _credentialCollection.InsertOneAsync(newCredential);
    }

    public void Initialize()
    {
        var connectionString =  _options.ConnectionString;
           
        var mongoClient = new MongoClient(connectionString);
        
        var mongoUrl = new MongoUrl(connectionString);
        
        var database = mongoClient.GetDatabase(mongoUrl.DatabaseName);

        _credentialCollection = database.GetCollection<Fido2Credential>("fido2_credentials");

    }

    public IFido2Credential CreateCredential(PublicKeyCredentialDescriptor publicKeyCredentialDescriptor, byte[] resultPublicKey,
        byte[] userId, uint resultCounter, string credType, DateTime regDate, Guid aaguid)
    {
        return new Fido2Credential
        {
            Id = ObjectId.GenerateNewId().ToString(),
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
        
        return Task.FromResult((IFido2Credential?)_credentialCollection.AsQueryable()
                            .FirstOrDefault(c => c.DescriptorIdBase64 == base64Id));
    }

    public  Task<List<IFido2Credential>> GetCredentialsByUserHandleAsync(byte[] userHandle, object cancellationToken)
    {
        var base64Handle = Convert.ToBase64String(userHandle);
        
        return Task.FromResult(_credentialCollection.AsQueryable().Where(c => c.UserHandleBase64 == base64Handle).ToList().Select(c => (IFido2Credential) c).ToList());
    }

    public async Task UpdateCounterAsync(byte[] credentialId, uint counter)
    {
        var cred = await GetCredentialByIdAsync(credentialId);

        if (cred != null)
        {
            cred.SignatureCounter = counter;

            await _credentialCollection.ReplaceOneAsync(c => c.Descriptor.Id == credentialId, (Fido2Credential) cred);
        }
    }
}