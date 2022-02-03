using Microsoft.Extensions.Localization;
using Nuages.AspNetIdentity;

using Nuages.Web.Exceptions;

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services.Manage;

public class ChangeEmailService : IChangeEmailService
{
    private readonly NuagesUserManager _userManager;
    private readonly IStringLocalizer _localizer;

    public ChangeEmailService(NuagesUserManager userManager, IStringLocalizer localizer)
    {
        _userManager = userManager;
        _localizer = localizer;
    }
    
    public async Task<ChangeEmailResultModel> ChangeEmailAsync(string userId, string email, string? token)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(email);
        
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null)
        {
            if (existing.Id == userId)
            {
                return new ChangeEmailResultModel
                {
                    Success = false,
                    Errors = new List<string> { _localizer["changeEmail:isNotChanged"]}
                };
            }
           
            return new ChangeEmailResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["changeEmail:emailAlreadyUsed"]}

            };
        }
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        var changeUserName = user.NormalizedEmail == user.NormalizedUserName;
        
        if (string.IsNullOrEmpty(token))
            token = await _userManager.GenerateChangeEmailTokenAsync(user, email);

        var res = await _userManager.ChangeEmailAsync(user, email, token);

        if (changeUserName)
        {
            res = await _userManager.SetUserNameAsync(user, email);
        }
        
        return new ChangeEmailResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Localize(_localizer)
        };
    }
}

public interface IChangeEmailService
{
    Task<ChangeEmailResultModel> ChangeEmailAsync(string userId, string email, string? token);
}

public class ChangeEmailResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}
