using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Sender.API.Sdk;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable InconsistentNaming

namespace Nuages.Identity.Services.Login;

public class SMSLoginService : ISMSLoginService
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly IMessageSender _sender;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<SMSLoginService> _logger;

    public SMSLoginService(NuagesUserManager userManager, NuagesSignInManager signInManager, IMessageSender sender, IStringLocalizer localizer, ILogger<SMSLoginService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _sender = sender;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<SendSMSCodeResultModel> SendCode()
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        return await SendCode(user.Id);
    }
    
    public async Task<SendSMSCodeResultModel> SendCode(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new SendSMSCodeResultModel
            {
                Success = true //Fake success
            };
        }

        if (string.IsNullOrEmpty(user.PhoneNumber) || !user.PhoneNumberConfirmed)
        {
            return new SendSMSCodeResultModel
            {
                Success = true //Fake success
            };
        }

        var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");

        var message = _localizer["passwordless:message", code];

        _logger.LogInformation($"Message : {message} No: {user.PhoneNumber}");
        
        await _sender.SendSmsAsync(user.PhoneNumber, message);
        
        return new SendSMSCodeResultModel
        {
            Success = true
        };
    }
}

public interface ISMSLoginService
{
    Task<SendSMSCodeResultModel> SendCode(string userId);
    Task<SendSMSCodeResultModel> SendCode();
}

public class SendSMSCodeResultModel
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SendSMSCodeModel
{
    public string? RecaptchaToken { get; set; }
}