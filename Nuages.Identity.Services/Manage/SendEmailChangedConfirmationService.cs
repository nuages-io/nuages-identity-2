using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Web;
using Nuages.Web.Exceptions;

namespace Nuages.Identity.Services.Manage;

public class SendEmailChangeConfirmationService : ISendEmailChangeConfirmationService
{
    private readonly IStringLocalizer _localizer;
    private readonly IMessageService _messageService;
    private readonly NuagesIdentityOptions _options;
    private readonly IRuntimeConfiguration _runtimeConfiguration;
    private readonly NuagesUserManager _userManager;

    public SendEmailChangeConfirmationService(NuagesUserManager userManager, IMessageService messageService,
        IOptions<NuagesIdentityOptions> options, IStringLocalizer localizer, IRuntimeConfiguration runtimeConfiguration)
    {
        _userManager = userManager;

        _messageService = messageService;
        _localizer = localizer;
        _runtimeConfiguration = runtimeConfiguration;
        _options = options.Value;
    }

    public async Task<SendEmailChangeResultModel> SendEmailChangeConfirmation(string userId, string email)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(email);


        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) throw new NotFoundException("UserNotFound");

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null)
        {
            if (existing.Id == userId)
                return new SendEmailChangeResultModel
                {
                    Success = false,
                    Errors = new List<string> { _localizer["changeEmail:isNotChanged"] }
                };

            return new SendEmailChangeResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["changeEmail:emailAlreadyUsed"] }
            };
        }

        var code = await _userManager.GenerateChangeEmailTokenAsync(user, email);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var url =
            $"{_options.Authority}/Account/ConfirmEmailChange?code={code}&userId={user.Id}&email={WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(email))}";

        _messageService.SendEmailUsingTemplate(email, "Confirm_Email_Change", new Dictionary<string, string>
        {
            { "Link", url }
        });

        return new SendEmailChangeResultModel
        {
            Code = _runtimeConfiguration.IsTest ? code : null,
            Url = _runtimeConfiguration.IsTest ? url : null,
            Success = true // Fake success
        };
    }
}

public interface ISendEmailChangeConfirmationService
{
    Task<SendEmailChangeResultModel> SendEmailChangeConfirmation(string userId, string email);
}

public class SendEmailChangeModel
{
    public string Email { get; set; } = string.Empty;
}

public class SendEmailChangeResultModel
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; set; }

    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}