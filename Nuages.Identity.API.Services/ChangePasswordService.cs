using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;
// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.API.Services;

public class ChangePasswordService : IChangePasswordService
{
    private readonly NuagesUserManager _userManager;
    private readonly IStringLocalizer _localizer;

    public ChangePasswordService(NuagesUserManager userManager, IStringLocalizer localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }
    
    public async Task<ChangePasswordResultModel> ChangePassword(ChangePasswordModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        var res = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        return new ChangePasswordResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Select(e => _localizer[e.Code].Value).ToList()
        };
    }
}

public interface IChangePasswordService
{
    Task<ChangePasswordResultModel> ChangePassword(ChangePasswordModel model);
}

public class ChangePasswordModel
{
    public string UserId { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
}

public class ChangePasswordResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}