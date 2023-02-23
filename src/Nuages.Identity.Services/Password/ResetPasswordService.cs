using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;


// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services.Password;

public class ResetPasswordService : IResetPasswordService
{
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<ResetPasswordService> _logger;
    private readonly IIdentityEventBus _identityEventBus;
    private readonly NuagesUserManager _userManager;

    public ResetPasswordService(NuagesUserManager userManager, IStringLocalizer localizer, ILogger<ResetPasswordService> logger, IIdentityEventBus identityEventBus)
    {
        _userManager = userManager;
        _localizer = localizer;
        _logger = logger;
        _identityEventBus = identityEventBus;
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
        {
            await _identityEventBus.PutEvent(IdentityEvents.ResetPasswordFailedUserNotFound, new { model.Email});
            
            return new ResetPasswordResultModel
            {
                Success = true //Fake Success
            };
        }

        var c = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));

        var result = await _userManager.ResetPasswordAsync(user, c, model.Password);
        if (result.Succeeded)
        {
            user.UserMustChangePassword = false;

           var updateRes = await _userManager.UpdateAsync(user);
            if (!updateRes.Succeeded)
            {
                _logger.LogError("{Error}",updateRes.Errors.First().Description);
            }
            
            await _identityEventBus.PutEvent(IdentityEvents.ResetPasswordSuccess, user);
            
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

        await _identityEventBus.PutEvent(IdentityEvents.ResetPasswordFailed, new { User = user, Result = res});
        
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