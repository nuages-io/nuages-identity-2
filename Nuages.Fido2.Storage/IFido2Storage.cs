using System.Collections.Concurrent;
using System.Text;
using Fido2NetLib;
using Fido2NetLib.Development;
using Nuages.Fido2.Models;

namespace Nuages.Fido2;

public interface IFido2Storage
{
    Task<Fido2User?> GetUserAsync(string userName);
    Task<List<IFido2Credential>> GetCredentialsByUserAsync(Fido2User user);
}

public class Fido2InMemoryStorage : IFido2Storage
{
    private readonly ConcurrentDictionary<string, Fido2User> _storedUsers = new();
    private readonly List<IFido2Credential> _storedCredentials = new();

    public Fido2User GetOrAddUser(string username, Func<Fido2User> addCallback)
    {
        return _storedUsers.GetOrAdd(username, addCallback());
    }

    public Task<Fido2User?> GetUserAsync(string username)
    {
        _storedUsers.TryGetValue(username, out var user);
        if (user == null)
        {
            user = new Fido2User
            {
                DisplayName = username,
                Name = username,
                Id = Encoding.UTF8.GetBytes(username) // byte representation of userID is required
            };
        }
        
        return Task.FromResult(user);
    }

    public Task<List<IFido2Credential>> GetCredentialsByUserAsync(Fido2User user)
    {
        return Task.FromResult( _storedCredentials.Where(c => c.UserId.AsSpan().SequenceEqual(user.Id)).ToList());
    }

    public IFido2Credential? GetCredentialById(byte[] id)
    {
        return _storedCredentials.FirstOrDefault(c => c.Descriptor.Id.AsSpan().SequenceEqual(id));
    }

    public Task<List<IFido2Credential>> GetCredentialsByUserHandleAsync(byte[] userHandle,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storedCredentials.Where(c => c.UserHandle.AsSpan().SequenceEqual(userHandle)).ToList());
    }

    public void UpdateCounter(byte[] credentialId, uint counter)
    {
        var cred = _storedCredentials.First(c => c.Descriptor.Id.AsSpan().SequenceEqual(credentialId));
        cred.SignatureCounter = counter;
    }

    public void AddCredentialToUser(Fido2User user, IFido2Credential credential)
    {
        credential.UserId = user.Id;
        _storedCredentials.Add(credential);
    }

    public Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] credentialId,
        CancellationToken cancellationToken = default)
    {
        // our in-mem storage does not allow storing multiple users for a given credentialId. Yours shouldn't either.
        var cred = _storedCredentials.FirstOrDefault(c => c.Descriptor.Id.AsSpan().SequenceEqual(credentialId));

        if (cred is null)
            return Task.FromResult(new List<Fido2User>());

        return Task.FromResult(_storedUsers.Where(u => u.Value.Id.SequenceEqual(cred.UserId)).Select(u => u.Value)
            .ToList());
    }
}