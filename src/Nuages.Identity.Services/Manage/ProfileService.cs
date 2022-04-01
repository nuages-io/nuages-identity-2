using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;

namespace Nuages.Identity.Services.Manage;

public class ProfileService : IProfileService
{
    private readonly IStringLocalizer _localizer;
    private readonly NuagesUserManager _userManager;

    public ProfileService(NuagesUserManager userManager, IStringLocalizer localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }

    public async Task<SaveProfileResultModel> SaveProfile(string id, SaveProfileModel model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) throw new NotFoundException("UserNotFound");

        user.LastName = model.LastName;
        user.FirstName = model.FirstName;

        var res = await _userManager.UpdateAsync(user);

        return new SaveProfileResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Localize(_localizer)
        };
    }
}

public interface IProfileService
{
    Task<SaveProfileResultModel> SaveProfile(string id, SaveProfileModel model);
}

public class SaveProfileModel
{
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
}

public class SaveProfileResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}