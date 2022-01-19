using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services.Manage.Admin;

public class AdminChangePasswordService : IAdminChangePasswordService
{
    private readonly NuagesUserManager _userManager;
    private readonly IStringLocalizer _localizer;

    public AdminChangePasswordService(NuagesUserManager userManager, IStringLocalizer localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }
    
    public async Task<AdminChangePasswordResultModel> AdminSetPasswordAsync(AdminChangePasswordModel model)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(model.UserId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(model.Password);
        
        if (model.Password != model.PasswordConfirmation)
        {
            return new AdminChangePasswordResultModel
            {
                Errors = new List<string>
                {
                   _localizer["resetPassword.passwordConfirmDoesNotMatch"]
                }
            };
        }
        
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        IdentityResult res;
        
        if (await _userManager.HasPasswordAsync(user))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            res = await _userManager.ResetPasswordAsync(user, token, model.Password);
        }
        else
        {
            res = await _userManager.AddPasswordAsync(user, model.Password);
        }
        
        if (res.Succeeded)
        {
            user.UserMustChangePassword = model.UserMustChangePassword;
        }

        if (model.SendByEmail)
        {
            //TODO
        }
        
        return new AdminChangePasswordResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Select(e => _localizer[$"identity.{e.Code}"].Value).ToList()
        };
    }
}

public interface IAdminChangePasswordService
{
    Task<AdminChangePasswordResultModel> AdminSetPasswordAsync(AdminChangePasswordModel model);
}

public class AdminChangePasswordModel
{
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordConfirmation { get; set; } = string.Empty;
    public bool UserMustChangePassword { get; set; }
    public bool SendByEmail { get; set; }
}

public class AdminChangePasswordResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}