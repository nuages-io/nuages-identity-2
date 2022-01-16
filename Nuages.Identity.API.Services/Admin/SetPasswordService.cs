using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.API.Services.Admin;

public class SetPasswordService : ISetPasswordService
{
    private readonly NuagesUserManager _userManager;
    private readonly IStringLocalizer _localizer;

    public SetPasswordService(NuagesUserManager userManager, IStringLocalizer localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }
    
    public async Task<SetPasswordResultModel> SetPasswordAsync(SetPasswordModel model)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(model.UserId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(model.Password);
        
        if (model.Password != model.PasswordConfirmation)
        {
            return new SetPasswordResultModel()
            {
                Errors = new List<string>()
                {
                   _localizer["resetPassword.passwordConfirmDoesNotMatch"]
                }
            };
        }

        
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var res = await _userManager.ResetPasswordAsync(user, token, model.Password);

        if (res.Succeeded)
        {
            user.UserMustChangePassword = model.UserMustChangePassword;
        }

        if (model.SendByEmail)
        {
            //TODO
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
    Task<SetPasswordResultModel> SetPasswordAsync(SetPasswordModel model);
}

public class SetPasswordModel
{
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordConfirmation { get; set; } = string.Empty;
    public bool UserMustChangePassword { get; set; }
    public bool SendByEmail { get; set; }
}

public class SetPasswordResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}