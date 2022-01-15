using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;
// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.API.Services;

public class ChangePhoneNumberService : IChangePhoneNumberService
{
    private readonly NuagesUserManager _userManager;
    private readonly IStringLocalizer _localizer;

    public ChangePhoneNumberService(NuagesUserManager userManager, IStringLocalizer localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }
    
    public async Task<ChangePhoneNumberResultModel> ChangePhoneNumberAsync(string userId, ChangePhoneNumberModel model)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(model.PhoneNumber);
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        var token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
        
        var res = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber, token);

        return new ChangePhoneNumberResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Select(e => _localizer[$"identity.{e.Code}"].Value).ToList()
        };
    }
}

public interface IChangePhoneNumberService
{
    Task<ChangePhoneNumberResultModel> ChangePhoneNumberAsync(string userId, ChangePhoneNumberModel model);
}

public class ChangePhoneNumberResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ChangePhoneNumberModel
{
    public string PhoneNumber { get; set; } = string.Empty;
}