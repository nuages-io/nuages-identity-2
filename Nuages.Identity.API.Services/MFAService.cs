// ReSharper disable InconsistentNaming

using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;

namespace Nuages.Identity.API.Services;

public class MFAService : IMFAService
{
    private readonly NuagesUserManager _userManager;
    private readonly UrlEncoder _urlEncoder;

    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    
    public MFAService(NuagesUserManager userManager, UrlEncoder urlEncoder)
    {
        _userManager = userManager;
        _urlEncoder = urlEncoder;
    }
    public Task<DisableMFAResultModel> DisableMFAAsync(DisableMFAModel model)
    {
        throw new NotImplementedException();
    }

    public Task<MFAResultModel> EnableMFAAsync(EnableMFAModel model)
    {
        throw new NotImplementedException();
    }

    public Task<MFAResultModel> ResetRecoveryCodesAsync(ResetRecoveryCodesModel model)
    {
        throw new NotImplementedException();
    }

    public async Task<GetMFAUrlResultModel>  GetMFAUrlAsync(GetMFAUrlModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        var (key, url) = await GetSharedKeyAndQrCodeUriAsync(user);

        return new GetMFAUrlResultModel
        {
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
            _urlEncoder.Encode("Microsoft.AspNetCore.Identity.UI"),
            _urlEncoder.Encode(email),
            unformattedKey);
    }
}

public interface IMFAService
{
    Task<DisableMFAResultModel> DisableMFAAsync(DisableMFAModel model);
    Task<MFAResultModel> EnableMFAAsync(EnableMFAModel model);
    Task<MFAResultModel> ResetRecoveryCodesAsync(ResetRecoveryCodesModel model);
    Task<GetMFAUrlResultModel> GetMFAUrlAsync(GetMFAUrlModel model);
}

public class GetMFAUrlResultModel
{
    public string Key { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class GetMFAUrlModel
{
    public string UserId { get; set; } = string.Empty;
}

public class ResetRecoveryCodesModel
{
}

public class EnableMFAModel
{
}

public class DisableMFAResultModel
{
}

public class DisableMFAModel
{
    
}

public class MFAResultModel
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Codes { get; set; } = new();
}

