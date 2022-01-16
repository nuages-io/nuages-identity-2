using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services.Manage;

public class ChangeUserNameService : IChangeUserNameService
{
    private readonly NuagesUserManager _userManager;
    private readonly IStringLocalizer _localizer;

    public ChangeUserNameService(NuagesUserManager userManager, IStringLocalizer localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }
    
    public async Task<ChangeUserNameResultModel> ChangeUserNameAsync(string userId, ChangeUserNameModel model)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(model.NewUserName);
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        var res = await _userManager.SetUserNameAsync(user, model.NewUserName);
        
        return new ChangeUserNameResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Select(e => _localizer[$"identity.{e.Code}"].Value).ToList()
        };
    }
}

public interface IChangeUserNameService
{
    Task<ChangeUserNameResultModel> ChangeUserNameAsync(string userId, ChangeUserNameModel model);
}

public class ChangeUserNameModel
{
    public string NewUserName { get; set; } = string.Empty;
}

public class ChangeUserNameResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}