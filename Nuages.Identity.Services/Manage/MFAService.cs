// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using Nuages.AspNetIdentity.Core;
using Nuages.Identity.Services.Email;
using Nuages.Web.Exceptions;

namespace Nuages.Identity.Services.Manage;

public class MFAService : IMFAService
{
    private readonly NuagesUserManager _userManager;
    private readonly UrlEncoder _urlEncoder;
    private readonly IStringLocalizer _localizer;
    private readonly IMessageService _messageService;
    private readonly NuagesIdentityOptions _options;

    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    private const string LoginProvider = "[AspNetUserStore]";
    private const string AuthenticatorKey = "AuthenticatorKey";
    private const string RecoveryCodes = "RecoveryCodes";

    public MFAService(NuagesUserManager userManager, UrlEncoder urlEncoder, IStringLocalizer localizer, IOptions<NuagesIdentityOptions> options, IMessageService messageService)
    {
        _userManager = userManager;
        _urlEncoder = urlEncoder;
        _localizer = localizer;
        _messageService = messageService;
        _options = options.Value;
    }
    public async Task<DisableMFAResultModel> DisableMFAAsync(string userId)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");
        
        var res = await _userManager.SetTwoFactorEnabledAsync(user, false);

        await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider,
            AuthenticatorKey);
        
        await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider,
            RecoveryCodes);
        
        var url = $"{_options.Authority}/account/manage/twoFactorAuthentication";

        _messageService.SendEmailUsingTemplate(user.Email, "2FA_Disabled", new Dictionary<string, string>
        {
            { "Link", url }
        });
        
        return new DisableMFAResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Localize(_localizer)
        };
    }

    public async Task<MFAResultModel> EnableMFAAsync(string userId, string code)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(code);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");
            
        var verificationCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2faTokenValid)
        {
            return new MFAResultModel
            {
                Errors = new List<string>{ _localizer["InvalidVerificationCode"]}
            };
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        var url = $"{_options.Authority}/account/manage/twoFactorAuthentication";

        _messageService.SendEmailUsingTemplate(user.Email, "2FA_Enabled", new Dictionary<string, string>
        {
            { "Link", url }
        });
        
        return new MFAResultModel
        {
            Success = true,
            Codes = recoveryCodes.ToList()
        };
    }

    public async Task<MFAResultModel> ResetRecoveryCodesAsync(string userId)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");
        
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return new MFAResultModel
        {
            Success = true,
            Codes = recoveryCodes.ToList()
        };
    }

    public async Task<GetMFAUrlResultModel>  GetMFAUrlAsync(string userId)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        var (key, url) = await GetSharedKeyAndQrCodeUriAsync(user);

        return new GetMFAUrlResultModel
        {
            Success = true,
            Key = key,
            Url = url
        };
    }
    
    private async Task<(string Key, string Url)> GetSharedKeyAndQrCodeUriAsync(NuagesApplicationUser user)
    {
        // Load the authenticator key & QR code URI to display on the form
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var email = await _userManager.GetEmailAsync(user);
        
        var url = GenerateQrCodeUri(email, key);

        return (key, url);
    }

 

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            AuthenticatorUriFormat,
            _urlEncoder.Encode(_options.Name),
            _urlEncoder.Encode(email),
            unformattedKey);
    }
    
    public async Task<List<string>> GetRecoveryCodes(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");
        
        var recoveryCode = await _userManager.GetAuthenticationTokenAsync(user, "[AspNetUserStore]", "RecoveryCodes");
        return recoveryCode?.Split(";").ToList() ?? new List<string>();
    }
}

public interface IMFAService
{
    Task<DisableMFAResultModel> DisableMFAAsync(string userId);
    Task<MFAResultModel> EnableMFAAsync(string userId, string code);
    Task<MFAResultModel> ResetRecoveryCodesAsync(string userId);
    Task<GetMFAUrlResultModel> GetMFAUrlAsync(string userId);
    Task<List<string>> GetRecoveryCodes(string userId);
}

public class GetMFAUrlResultModel
{
    public bool Success { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    [ExcludeFromCodeCoverage]
    public List<string> Errors { get; set; } = new();
}

[ExcludeFromCodeCoverage]
public class EnableMFAModel
{
    public string Code { get; set; } = string.Empty;
}

public class DisableMFAResultModel
{
    public bool Success { get; set; }
    [ExcludeFromCodeCoverage]
    public List<string> Errors { get; set; } = new();
}


public class MFAResultModel
{
    public bool Success { get; set; }
    
    public List<string> Errors { get; set; } = new();
    public List<string> Codes { get; set; } = new();
}

