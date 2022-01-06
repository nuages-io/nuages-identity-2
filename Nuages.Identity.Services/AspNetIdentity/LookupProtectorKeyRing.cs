using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.AspNetIdentity;

public class LookupProtectorKeyRing : ILookupProtectorKeyRing
{
    public IEnumerable<string> GetAllKeyIds()
    {
        return new [] { Guid.Empty.ToString() };
    }

    public string CurrentKeyId => GetAllKeyIds().First();

    public string this[string keyId] => CurrentKeyId;
}