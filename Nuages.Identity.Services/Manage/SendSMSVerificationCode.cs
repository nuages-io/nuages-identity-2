using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Web.Exceptions;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable InconsistentNaming

namespace Nuages.Identity.Services.Manage;

public class SendSMSVerificationCodeService : ISendSMSVerificationCodeService
{
    private readonly NuagesUserManager _userManager;
    private readonly IMessageService _sender;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<SendSMSVerificationCodeService> _logger;

    public SendSMSVerificationCodeService(NuagesUserManager userManager, IMessageService sender, IStringLocalizer localizer, ILogger<SendSMSVerificationCodeService> logger)
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
            throw new NotFoundException("UswerNotFound");
        }

        phoneNumber = phoneNumber.Replace("+", "").Replace("+", " ").Replace("+", "-");

        var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);

        var message = _localizer["changePhoneNumber:smsMessage", code];

        _logger.LogInformation($"Message : {message} No: {phoneNumber}");
        
        _sender.SendSms(phoneNumber, message);
        
        return new SendSMSVerificationCodeResultModel
        {
            Success = true
        };
    }
}

public interface ISendSMSVerificationCodeService
{
    // ReSharper disable once UnusedMemberInSuper.Global
    Task<SendSMSVerificationCodeResultModel> SendCode(string userId, string phoneNumber);
}

public class SendSMSVerificationCodeResultModel
{
    public bool Success { get; set; }
    [ExcludeFromCodeCoverage]
    public List<string> Errors { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class SendSMSVerificationCodeModel
{
    public string PhoneNumber { get; set; } = string.Empty;
}
