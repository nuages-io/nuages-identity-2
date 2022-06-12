using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Web.Exceptions;

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services.Admin;

public class AdminChangePasswordService : IAdminChangePasswordService
{
    private readonly IStringLocalizer _localizer;
    private readonly IMessageService _messageService;
    private readonly NuagesUserManager _userManager;

    public AdminChangePasswordService(NuagesUserManager userManager, IStringLocalizer localizer,
        IMessageService messageService)
    {
        _userManager = userManager;
        _localizer = localizer;
        _messageService = messageService;
    }


    public async Task<ChangePasswordResultModel> AdminChangePasswordAsync(string userId, string newPassword,
        string newPasswordConfirmation, bool mustChangePassword, bool sendByEmail, string? token)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(newPassword);

        if (newPassword != newPasswordConfirmation)
            return new ChangePasswordResultModel
            {
                Errors = new List<string>
                {
                    _localizer["resetPassword.passwordConfirmDoesNotMatch"]
                }
            };

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        IdentityResult res;

        if (await _userManager.HasPasswordAsync(user))
        {
            if (string.IsNullOrEmpty(token))
                token = await _userManager.GeneratePasswordResetTokenAsync(user);

            res = await _userManager.ResetPasswordAsync(user, token, newPassword);
        }
        else
        {
            res = await _userManager.AddPasswordAsync(user, newPassword);
        }

        if (res.Succeeded) user.UserMustChangePassword = mustChangePassword;

        if (sendByEmail)
            _messageService.SendEmailUsingTemplate(user.Email, "Password_Was_Changed_ByAdmin",
                new Dictionary<string, string>
                {
                    { "UserName", user.UserName },
                    { "Password", newPassword },
                    { "MustCHangePassword", mustChangePassword.ToString() }
                });

        return new ChangePasswordResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Localize(_localizer)
        };
    }
}

public interface IAdminChangePasswordService
{

    Task<ChangePasswordResultModel> AdminChangePasswordAsync(string userId, string newPassword,
        string newPasswordConfirmation, bool mustChangePassword, bool sendByEmail, string? token);
}

public class ChangePasswordResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}