using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nuages.AspNetIdentity;

using Nuages.Identity.Services.Email;
using Nuages.Web.Exceptions;

namespace Nuages.Identity.Services.Manage;

public class SendEmailChangeConfirmationService : ISendEmailChangeConfirmationService
{
    private readonly NuagesUserManager _userManager;
    private readonly IMessageService _messageService;
    private readonly IStringLocalizer _localizer;
    private readonly NuagesIdentityOptions _options;

    public SendEmailChangeConfirmationService(NuagesUserManager userManager, IMessageService messageService, 
        IOptions<NuagesIdentityOptions> options, IStringLocalizer localizer)
    {
        _userManager = userManager;
       
        _messageService = messageService;
        _localizer = localizer;
        _options = options.Value;
    }
    
    public async Task<SendEmailChangeResultModel> SendEmailChangeConfirmation(string userId, string email)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(email);

           
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("UserNotFound");
        }
        
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null)
        {
            if (existing.Id == userId)
            {
                return new SendEmailChangeResultModel
                {
                    Success = false,
                    Errors = new List<string> { _localizer["changeEmail:isNotChanged"]}
                };
            }
           
            return new SendEmailChangeResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["changeEmail:emailAlreadyUsed"]}

            };
        }
     
       
        var code = await _userManager.GenerateChangeEmailTokenAsync(user, email);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        var url = $"{_options.Authority}/Account/ConfirmEmailChange?code={code}&userId={user.Id}&email={WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(email))}";
        
        _messageService.SendEmailUsingTemplate(email, "Confirm_Email_Change", new Dictionary<string, string>
        {
            { "Link", url }
        });
        
        return new SendEmailChangeResultModel
        {
            Success = true // Fake success
        };
    }
}

public interface ISendEmailChangeConfirmationService
{
    Task<SendEmailChangeResultModel> SendEmailChangeConfirmation(string userId, string email);
}

[ExcludeFromCodeCoverage]
public class SendEmailChangeModel
{
    public string Email { get; set; } = string.Empty;
}

public class SendEmailChangeResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}