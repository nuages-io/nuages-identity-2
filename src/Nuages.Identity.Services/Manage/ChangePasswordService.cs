using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Web.Exceptions;

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services.Manage;

public class ChangePasswordService : IChangePasswordService
{
    private readonly IStringLocalizer _localizer;
    private readonly IMessageService _messageService;
    private readonly IIdentityEventBus _identityEventBus;
    private readonly NuagesUserManager _userManager;

    public ChangePasswordService(NuagesUserManager userManager, IStringLocalizer localizer,
        IMessageService messageService, IIdentityEventBus identityEventBus)
    {
        _userManager = userManager;
        _localizer = localizer;
        _messageService = messageService;
        _identityEventBus = identityEventBus;
    }

    public async Task<ChangePasswordResultModel> AddPasswordAsync(string userid, string newPassword,
        string newPasswordConfirmation)
    {
        return await ChangePasswordAsync(userid, "no_password", newPassword, newPasswordConfirmation);
    }

    public async Task<ChangePasswordResultModel> ChangePasswordAsync(string userid, string currentPassword,
        string newPassword,
        string newPasswordConfirmation)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userid);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(currentPassword);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(newPassword);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(newPasswordConfirmation);

        if (newPassword != newPasswordConfirmation)
            return new ChangePasswordResultModel
            {
                Errors = new List<string>
                {
                    _localizer["resetPassword.passwordConfirmDoesNotMatch"]
                }
            };

        var user = await _userManager.FindByIdAsync(userid);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        IdentityResult res;

        if (await _userManager.HasPasswordAsync(user))
        {
            res = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (res.Succeeded)
            {
                await _identityEventBus.PutEvent(IdentityEvents.PasswordChanged,user);
                
                _messageService.SendEmailUsingTemplate(user.Email, "Password_Was_Changed",
                    new Dictionary<string, string>());
            }
            
        }
        else
        {
            res = await _userManager.AddPasswordAsync(user, newPassword);

            if (res.Succeeded)
            {
                await _identityEventBus.PutEvent(IdentityEvents.PasswordAdded,user);
                
                _messageService.SendEmailUsingTemplate(user.Email, "Password_Was_Added", new Dictionary<string, string>
                {
                    // { "PhoneNumber", phoneNumber }
                });
            }
               
        }

        return new ChangePasswordResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Localize(_localizer)
        };
    }

   
}

public interface IChangePasswordService
{
    Task<ChangePasswordResultModel> AddPasswordAsync(string userid, string newPassword,
        string newPasswordConfirmation);

    Task<ChangePasswordResultModel> ChangePasswordAsync(string userid, string currentPassword, string newPassword,
        string newPasswordConfirmation);

}

public class ChangePasswordModel
{
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
    public string NewPasswordConfirm { get; set; } = string.Empty;
}

public class ChangePasswordResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}