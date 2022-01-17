using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Sender.API.Sdk;

namespace Nuages.Identity.Services;

public class SMSLoginService : ISMSLoginService
{
    private readonly NuagesUserManager _userManager;
    private readonly IEmailSender _sender;

    public SMSLoginService(NuagesUserManager userManager, IEmailSender sender)
    {
        _userManager = userManager;
        _sender = sender;
    }
    
    public async Task<SendSMSCodeResultModel> SendCode(SendSMSCodeModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
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


        await _sender.SendEmailUsingTemplateAsync(user.Email, "SMS_Login", new Dictionary<string, string>
        {
            { "Code", code }
        });
        
        return new SendSMSCodeResultModel
        {
            Success = true
        };
    }
}

public interface ISMSLoginService
{
    Task<SendSMSCodeResultModel> SendCode(SendSMSCodeModel model);
}

public class SendSMSCodeResultModel
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SendSMSCodeModel
{
    public string Email { get; set; } = string.Empty;
}