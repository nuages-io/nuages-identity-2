using System.Text.Json.Serialization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Web;
using Nuages.Web.Exceptions;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable InconsistentNaming

namespace Nuages.Identity.Services.Login;

public class SMSSendCodeService : ISMSSendCodeService
{
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<SMSSendCodeService> _logger;
    private readonly NuagesIdentityOptions _options;
    private readonly IRuntimeConfiguration _runtimeConfiguration;
    private readonly IMessageService _sender;
    private readonly NuagesSignInManager _signInManager;
    private readonly NuagesUserManager _userManager;

    public SMSSendCodeService(NuagesUserManager userManager, NuagesSignInManager signInManager, IMessageService sender,
        IOptions<NuagesIdentityOptions> options, IStringLocalizer localizer, ILogger<SMSSendCodeService> logger,
        IRuntimeConfiguration runtimeConfiguration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _sender = sender;
        _localizer = localizer;
        _logger = logger;
        _runtimeConfiguration = runtimeConfiguration;
        _options = options.Value;
    }

    public async Task<SendSMSCodeResultModel> SendCode()
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

        return await SendCode(user.Id);
    }

    public async Task<SendSMSCodeResultModel> SendCode(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) throw new NotFoundException("UserNotFounc");

        if (string.IsNullOrEmpty(user.PhoneNumber) || !user.PhoneNumberConfirmed)
            return new SendSMSCodeResultModel
            {
                Success = true //Fake success
            };

        var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");

        var message = _localizer["magicLink:message", code, _options.Name];

        _logger.LogInformation($"Message : {message} No: {user.PhoneNumber}");

        _sender.SendSms(user.PhoneNumber, message);

        return new SendSMSCodeResultModel
        {
            Code = _runtimeConfiguration.IsTest ? code : null,
            Success = true
        };
    }
}

public interface ISMSSendCodeService
{
    // ReSharper disable once UnusedMemberInSuper.Global
    Task<SendSMSCodeResultModel> SendCode(string userId);
    Task<SendSMSCodeResultModel> SendCode();
}

public class SendSMSCodeResultModel
{
    public bool Success { get; set; }

    // ReSharper disable once CollectionNeverQueried.Global
    public List<string> Errors { get; set; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }
}