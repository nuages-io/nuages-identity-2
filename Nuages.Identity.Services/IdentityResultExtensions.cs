using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace Nuages.Identity.Services;

public static class IdentityResultExtensions
{
    public static List<string> Localize(this IEnumerable<IdentityError> list, IStringLocalizer localizer)
    {
        return list.Select(e => localizer[$"identity.{e.Code}"].Value).ToList();
    }
}