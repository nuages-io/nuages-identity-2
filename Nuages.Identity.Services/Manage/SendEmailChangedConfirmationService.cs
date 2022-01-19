using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Sender.API.Sdk;
using Nuages.Web.Exceptions;

namespace Nuages.Identity.Services.Manage;

public class SendEmailChangedConfirmationService : ISendEmailChangedConfirmationService
{
    private readonly NuagesUserManager _userManager;
    private readonly IMessageSender _messageSender;
    private readonly IStringLocalizer _localizer;
    private readonly NuagesIdentityOptions _options;

    public SendEmailChangedConfirmationService(NuagesUserManager userManager, IMessageSender messageSender, 
        IOptions<NuagesIdentityOptions> options, IStringLocalizer localizer)
    {
        _userManager = userManager;
       
        _messageSender = messageSender;
        _localizer = localizer;
        _options = options.Value;
    }
    
    public async Task<SendEmailConfirmationResultModel> SendEmailConfirmation(string userId, string email)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(email);

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null)
        {
            if (existing.Id == userId)
            {
                return new SendEmailConfirmationResultModel
                {
                    Success = false,
                    Errors = new List<string>() { _localizer["changeEmail:isNotChanged"]}
                };
            }
           
            return new SendEmailConfirmationResultModel
            {
                Success = false,
                Errors = new List<string>() { _localizer["changeEmail:emailAlreadyUsed"]}

            };
        }
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new SendEmailConfirmationResultModel
            {
                Success = true // Fake success
            };
        }
       
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        var url = $"{_options.Authority}Account/ConfirmEmailChange?code={code}&userId={user.Id}&email={email}";
        
        await _messageSender.SendEmailUsingTemplateAsync(user.Email, "Confirm_Email_Changed", new Dictionary<string, string>
        {
            { "Link", url }
        });
        
        return new SendEmailConfirmationResultModel
        {
            Success = true // Fake success
        };
    }
}

public interface ISendEmailChangedConfirmationService
{
    Task<SendEmailConfirmationResultModel> SendEmailConfirmation(string userId, string email);
}

// ReSharper disable once ClassNeverInstantiated.Global
public class SendEmailConfirmationChangedModel
{
    public string? Email { get; set; }
}

public class SendEmailConfirmationResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}