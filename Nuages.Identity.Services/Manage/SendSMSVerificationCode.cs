using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Sender.API.Sdk;
using Nuages.Web.Exceptions;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable InconsistentNaming

namespace Nuages.Identity.Services.Manage;

public class SendSMSVerificationCode : ISendSMSVerificationCode
{
    private readonly NuagesUserManager _userManager;
    private readonly IMessageService _sender;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<SendSMSVerificationCode> _logger;

    public SendSMSVerificationCode(NuagesUserManager userManager, IMessageService sender, IStringLocalizer localizer, ILogger<SendSMSVerificationCode> logger)
    {
        _userManager = userManager;
        _sender = sender;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<SendSMSVerificationCodeResultModel> SendCode(string userId, string phoneNumber)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(phoneNumber);
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new SendSMSVerificationCodeResultModel
            {
                Success = true //Fake success
            };
        }

        phoneNumber = phoneNumber.Replace("+", "").Replace("+", " ").Replace("+", "-");
        
        // var res = await _userManager.SetPhoneNumberAsync(user, phoneNumber);
        // if (!res.Succeeded)
        // {
        //     return new SendSMSVerificationCodeResultModel
        //     {
        //         Success = false,
        //         Errors = res.Errors.Select(e => _localizer[$"identity.{e.Code}"].Value).ToList()
        //     };
        // }

        var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);

        var message = _localizer["changePhoneNumber:smsMessage", code];

        _logger.LogInformation($"Message : {message} No: {phoneNumber}");
        
        await _sender.SendSmsAsync(phoneNumber, message);
        
        return new SendSMSVerificationCodeResultModel
        {
            Success = true
        };
    }
}

public interface ISendSMSVerificationCode
{
    // ReSharper disable once UnusedMemberInSuper.Global
    Task<SendSMSVerificationCodeResultModel> SendCode(string userId, string phoneNumber);
}

public class SendSMSVerificationCodeResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class SendSMSVerificationCodeModel
{
    public string PhoneNumber { get; set; } = string.Empty;
}
