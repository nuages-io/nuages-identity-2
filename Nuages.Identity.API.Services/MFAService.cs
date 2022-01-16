// ReSharper disable InconsistentNaming

using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;

namespace Nuages.Identity.API.Services;

public class MFAService : IMFAService
{
    private readonly NuagesUserManager _userManager;
    private readonly UrlEncoder _urlEncoder;
    private readonly IStringLocalizer _localizer;
    private readonly NuagesIdentityOptions _options;

    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    private const string LoginProvider = "[AspNetUserStore]";
    private const string AuthenticatorKey = "AuthenticatorKey";
    private const string RecoveryCodes = "RecoveryCodes";

    public MFAService(NuagesUserManager userManager, UrlEncoder urlEncoder, IStringLocalizer localizer, IOptions<NuagesIdentityOptions> options)
    {
        _userManager = userManager;
        _urlEncoder = urlEncoder;
        _localizer = localizer;
        _options = options.Value;
    }
    public async Task<DisableMFAResultModel> DisableMFAAsync(string userId)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");
        
        var res = await _userManager.SetTwoFactorEnabledAsync(user, false);

        var res2 = await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider,
            AuthenticatorKey);
        
        var res3 = await _userManager.RemoveAuthenticationTokenAsync(user, LoginProvider,
            RecoveryCodes);
        
        return new DisableMFAResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Select(e => _localizer[$"identity.{e.Code}"].Value).ToList()
        };
    }

    public async Task<MFAResultModel> EnableMFAAsync(string userId, EnableMFAModel model)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(model.Code);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");
            
        var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2faTokenValid)
        {
            return new MFAResultModel
            {
                Errors = new List<string>{ "InvalidVerificationCode" }
            };
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

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
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var key = FormatKey(unformattedKey);

        var email = await _userManager.GetEmailAsync(user);
        
        var url = GenerateQrCodeUri(email, unformattedKey);

        return (key, url);
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
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
}

public interface IMFAService
{
    Task<DisableMFAResultModel> DisableMFAAsync(string userId);
    Task<MFAResultModel> EnableMFAAsync(string userId, EnableMFAModel model);
    Task<MFAResultModel> ResetRecoveryCodesAsync(string userId);
    Task<GetMFAUrlResultModel> GetMFAUrlAsync(string userId);
}

public class GetMFAUrlResultModel
{
    public bool Success { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}


public class EnableMFAModel
{
    public string Code { get; set; } = string.Empty;
}

public class DisableMFAResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}


public class MFAResultModel
{
    public bool Success { get; set; }
    
    public List<string> Errors { get; set; } = new();
    public List<string> Codes { get; set; } = new();
}

