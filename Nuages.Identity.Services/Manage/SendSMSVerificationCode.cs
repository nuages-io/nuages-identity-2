using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Sender.API.Sdk;
using Nuages.Web.Exceptions;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable InconsistentNaming

namespace Nuages.Identity.Services.Manage;

public class SendSMSVerificationCode : ISendSMSVerificationCode
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly IMessageSender _sender;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<SendSMSVerificationCode> _logger;

    public SendSMSVerificationCode(NuagesUserManager userManager, NuagesSignInManager signInManager, IMessageSender sender, IStringLocalizer localizer, ILogger<SendSMSVerificationCode> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _sender = sender;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<endSMSVerificationCodeResultModel> SendCode(string userId, string phoneNumber)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(phoneNumber);
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new endSMSVerificationCodeResultModel
            {
                Success = true //Fake success
            };
        }

        var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");

        var message = _localizer["changePhoneNumber:smsMessage", code];

        _logger.LogInformation($"Message : {message} No: {phoneNumber}");
        
        await _sender.SendSmsAsync(user.PhoneNumber, message);
        
        return new endSMSVerificationCodeResultModel
        {
            Success = true
        };
    }
}

public interface ISendSMSVerificationCode
{
    // ReSharper disable once UnusedMemberInSuper.Global
    Task<endSMSVerificationCodeResultModel> SendCode(string userId, string phoneNumber);
}

public class endSMSVerificationCodeResultModel
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
