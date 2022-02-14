using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;


// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services.Password;

public class ResetPasswordService : IResetPasswordService
{
    private readonly IStringLocalizer _localizer;
    private readonly NuagesUserManager _userManager;

    public ResetPasswordService(NuagesUserManager userManager, IStringLocalizer localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }

    public async Task<ResetPasswordResultModel> Reset(ResetPasswordModel model)
    {
        if (model.Password != model.PasswordConfirm)
            return new ResetPasswordResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer.GetString("resetPassword:passwordConfirmDoesNotMatch") }
            };

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return new ResetPasswordResultModel
            {
                Success = true //Fake Success
            };

        var c = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));

        var result = await _userManager.ResetPasswordAsync(user, c, model.Password);
        if (result.Succeeded)
        {
            user.UserMustChangePassword = false;

            await _userManager.UpdateAsync(user);

            return new ResetPasswordResultModel
            {
                Success = true
            };
        }

        var res = new ResetPasswordResultModel
        {
            Success = false,
            Errors = result.Errors.Localize(_localizer)
        };

        return res;
    }
}

public interface IResetPasswordService
{
    Task<ResetPasswordResultModel> Reset(ResetPasswordModel model);
}

public class ResetPasswordModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordConfirm { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class ResetPasswordResultModel
{
    public bool Success { get; set; }

    // ReSharper disable once CollectionNeverQueried.Global
    public List<string> Errors { get; set; } = new();
}