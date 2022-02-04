using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

using Nuages.AspNetIdentity.Core;
using Nuages.Web.Exceptions;

namespace Nuages.Identity.Services.Email;

public class SendEmailConfirmationService : ISendEmailConfirmationService
{
    private readonly NuagesUserManager _userManager;
    private readonly IMessageService _messageService;
    private readonly NuagesIdentityOptions _options;

    public SendEmailConfirmationService(NuagesUserManager userManager, IMessageService messageService, IOptions<NuagesIdentityOptions> options)
    {
        _userManager = userManager;
       
        _messageService = messageService;
        _options = options.Value;
    }
    
    public async Task<SendEmailConfirmationResultModel> SendEmailConfirmation(SendEmailConfirmationModel model)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(model.Email);

        var email = model.Email;
        
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return new SendEmailConfirmationResultModel
            {
                Success = true // Fake success
            };
        }
       
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        var url = $"{_options.Authority}/Account/ConfirmEmail?code={code}&userId={user.Id}";
        
        _messageService.SendEmailUsingTemplate(user.Email, "Confirm_Email", new Dictionary<string, string>
        {
            { "Link", url }
        });
        
        return new SendEmailConfirmationResultModel
        {
            Success = true // Fake success
            
        };
    }
}

public interface ISendEmailConfirmationService
{
    Task<SendEmailConfirmationResultModel> SendEmailConfirmation(SendEmailConfirmationModel model);
}

// ReSharper disable once ClassNeverInstantiated.Global
public class SendEmailConfirmationModel
{
    public string? Email { get; set; }
}

public class SendEmailConfirmationResultModel
{
    public bool Success { get; set; }
    [ExcludeFromCodeCoverage]
    public string? Message { get; set; }
}