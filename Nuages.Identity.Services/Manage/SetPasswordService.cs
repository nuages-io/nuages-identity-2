using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services.Manage;

public class SetPasswordService : ISetPasswordService
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly IStringLocalizer _localizer;

    public SetPasswordService(NuagesUserManager userManager, NuagesSignInManager signInManager, IStringLocalizer localizer)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _localizer = localizer;
    }
    
    public async Task<SetPasswordResultModel> SetPasswordAsync(string userId, SetPasswordModel model)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(model.NewPassword);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(model.NewPasswordConfirm);

        if (model.NewPassword != model.NewPasswordConfirm)
        {
            return new SetPasswordResultModel
            {
                Errors = new List<string>
                {
                    _localizer["resetPassword.passwordConfirmDoesNotMatch"]
                }
            };
        }
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        var res = await _userManager.AddPasswordAsync(user, model.NewPassword);

        if (res.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
        }
        
        return new SetPasswordResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Select(e => _localizer[$"identity.{e.Code}"].Value).ToList()
        };
    }
}

public interface ISetPasswordService
{
    Task<SetPasswordResultModel> SetPasswordAsync(string userid, SetPasswordModel model);
}

public class SetPasswordModel
{
    
    public string NewPassword { get; set; } = string.Empty;
    public string NewPasswordConfirm { get; set; } = string.Empty;
}

public class SetPasswordResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}