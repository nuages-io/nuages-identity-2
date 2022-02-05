using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Extensions.Options;
using Nuages.AspNetIdentity.Core;
using OpenIddict.Abstractions;

namespace Nuages.Identity.UI.OpenIdDict;

public class AudienceValidator : IAudienceValidator
{
    private readonly NuagesIdentityOptions _options;

    public AudienceValidator(IOptions<NuagesIdentityOptions> options)
    {
        _options = options.Value;
    }
    
    public string? CheckAudience(OpenIddictRequest openIdDictRequest, IPrincipal? principal)
    {
        if (openIdDictRequest.Audiences != null)
        {
            foreach (var audience in openIdDictRequest.Audiences)
            {
                if (IsValidAudience(audience))
                {
                    (principal!.Identity as ClaimsIdentity ?? throw new InvalidOperationException())
                        .AddClaim("aud", audience);
                }
                else
                {
                    return "Invalid Audience provided";
                }
            }
        }
        else
        {
            if (HasAudiences)
                return "Audience must be provided";
        }

        return null;
    }

    private bool IsValidAudience(string audience)
    {
        return _options.Audiences != null && _options.Audiences.Contains(audience);
    }

    private bool HasAudiences => _options.Audiences != null && _options.Audiences.Any();
}

public interface IAudienceValidator
{
    string? CheckAudience(OpenIddictRequest openIdDictRequest, IPrincipal? principal);
}