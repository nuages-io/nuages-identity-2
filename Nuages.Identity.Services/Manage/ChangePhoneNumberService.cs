using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services.Manage;

public class ChangePhoneNumberService : IChangePhoneNumberService
{
    private readonly NuagesUserManager _userManager;
    private readonly IStringLocalizer _localizer;

    public ChangePhoneNumberService(NuagesUserManager userManager, IStringLocalizer localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }
    
    public async Task<ChangePhoneNumberResultModel> ChangePhoneNumberAsync(string userId, string phoneNumber, string? token)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(phoneNumber);
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        if (string.IsNullOrEmpty(token))
            token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
        
        var res = await _userManager.ChangePhoneNumberAsync(user, phoneNumber, token);

        return new ChangePhoneNumberResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Select(e => _localizer[$"identity.{e.Code}"].Value).ToList()
        };
    }
}

public interface IChangePhoneNumberService
{
    Task<ChangePhoneNumberResultModel> ChangePhoneNumberAsync(string userId,string phone, string? token);
}

public class ChangePhoneNumberResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ChangePhoneNumberModel
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}