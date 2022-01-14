using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.API.Services.Admin;

public class SetPassworService : ISetPasswordService
{
    private readonly NuagesUserManager _userManager;
    private readonly IStringLocalizer _localizer;

    public SetPassworService(NuagesUserManager userManager, IStringLocalizer localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }
    
    public async Task<SetPasswordResultModel> SetPassword(SetPasswordModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var res = await _userManager.ResetPasswordAsync(user, token, model.Password);

        return new SetPasswordResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Select(e => _localizer[e.Code].Value).ToList()
        };
    }
}

public interface ISetPasswordService
{
    Task<SetPasswordResultModel> SetPassword(SetPasswordModel model);
}

public class SetPasswordModel
{
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SetPasswordResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}