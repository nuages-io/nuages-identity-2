using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services;


public class LookupProtector : ILookupProtector
{
    public string Protect(string keyId, string data)
    {
        return string.IsNullOrEmpty(data) ? "" : new string(data.Reverse().ToArray());
    }

    public string Unprotect(string keyId, string data)
    {
        return string.IsNullOrEmpty(data) ? "" : new string(data.Reverse().ToArray());
    }
}