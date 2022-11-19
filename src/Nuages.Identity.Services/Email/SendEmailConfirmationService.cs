using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Web;
using Nuages.Web.Exceptions;

namespace Nuages.Identity.Services.Email;

public class SendEmailConfirmationService : ISendEmailConfirmationService
{
    private readonly IMessageService _messageService;
    private readonly NuagesIdentityOptions _options;
    private readonly IRuntimeConfiguration _runtimeConfiguration;
    private readonly IIdentityEventBus _identityEventBus;
    private readonly NuagesUserManager _userManager;

    public SendEmailConfirmationService(NuagesUserManager userManager, IMessageService messageService,
        IOptions<NuagesIdentityOptions> options, IRuntimeConfiguration runtimeConfiguration, IIdentityEventBus identityEventBus)
    {
        _userManager = userManager;

        _messageService = messageService;
        _runtimeConfiguration = runtimeConfiguration;
        _identityEventBus = identityEventBus;
        _options = options.Value;
    }

    public async Task<SendEmailConfirmationResultModel> SendEmailConfirmation(SendEmailConfirmationModel model)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(model.Email);

        var email = model.Email;

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            await _identityEventBus.PutEvent(IdentityEvents.ConfirmationEmailFailed, model);
            
            return new SendEmailConfirmationResultModel
            {
                Success = true // Fake success
            };
        }
          

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var url = $"{_options.Authority}Account/ConfirmEmail?code={code}&userId={user.Id}";

        _messageService.SendEmailUsingTemplate(user.Email!, "Confirm_Email", new Dictionary<string, string>
        {
            { "Link", url }
        });

        await _identityEventBus.PutEvent(IdentityEvents.ConfirmationEmailSent, user);
        
        return new SendEmailConfirmationResultModel
        {
            Url = _runtimeConfiguration.IsTest ? url : null,
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
    public string? Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; set; }
}