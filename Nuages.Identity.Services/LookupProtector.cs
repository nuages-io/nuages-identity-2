using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services;


public class LookupProtector : ILookupProtector
{
    public string Protect(string keyId, string data)
    {
        if (string.IsNullOrEmpty(data))
            return "";

        return new string(data?.Reverse().ToArray());
    }

    public string Unprotect(string keyId, string data)
    {
        if (string.IsNullOrEmpty(data))
            return "";
        
        return new string(data?.Reverse().ToArray());
    }
}

public class LookupProtectorKeyRing : ILookupProtectorKeyRing
{
    public IEnumerable<string> GetAllKeyIds()
    {
        return new [] { Guid.Empty.ToString() };
    }

    public string CurrentKeyId
    {
        get
        {
            return GetAllKeyIds().First();
        }
    }

    public string this[string keyId] => CurrentKeyId;
}