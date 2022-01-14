using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;
// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.API.Services;

public class ChangeUserNameService : IChangeUserNameService
{
    private readonly NuagesUserManager _userManager;
    private readonly IStringLocalizer _localizer;

    public ChangeUserNameService(NuagesUserManager userManager, IStringLocalizer localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }
    
    public async Task<ChangeUserNameResultModel> ChangeUserName(ChangeUserNameModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        var res = await _userManager.SetUserNameAsync(user, model.NewUserName);
        
        return new ChangeUserNameResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Select(e => _localizer[e.Code].Value).ToList()
        };
    }
}

public interface IChangeUserNameService
{
    Task<ChangeUserNameResultModel> ChangeUserName(ChangeUserNameModel model);
}

public class ChangeUserNameModel
{
    public string UserId { get; set; } = string.Empty;
    public string NewUserName { get; set; } = string.Empty;
}

public class ChangeUserNameResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}