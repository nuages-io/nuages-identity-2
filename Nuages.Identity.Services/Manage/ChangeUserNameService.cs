using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;
using Nuages.Web.Utilities;

// ReSharper disable ClassNeverInstantiated.Global

namespace Nuages.Identity.Services.Manage;

public class ChangeUserNameService : IChangeUserNameService
{
    private readonly NuagesUserManager _userManager;
    private readonly IStringLocalizer _localizer;
    private readonly IEmailValidator _emailValidator;

    public ChangeUserNameService(NuagesUserManager userManager, IStringLocalizer localizer, IEmailValidator emailValidator)
    {
        _userManager = userManager;
        _localizer = localizer;
        _emailValidator = emailValidator;
    }
    
    public async Task<ChangeUserNameResultModel> ChangeUserNameAsync(string userId, string newUserName)
    {
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(userId);
        ArgumentNullOrEmptyException.ThrowIfNullOrEmpty(newUserName);

        if (_emailValidator.IsValidEmail(newUserName))
        {
            return new ChangeUserNameResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["changeUsername:mustNotBeAnEmail"] }
            };
        }

        var existing = await _userManager.FindByNameAsync(newUserName);
        if (existing != null)
        {
            if (existing.Id == userId)
            {
                return new ChangeUserNameResultModel
                {
                    Success = false,
                    Errors = new List<string> { _localizer["changeUsername:isNotChanged"]}
                };
            }
           
            return new ChangeUserNameResultModel
            {
                Success = false,
                Errors = new List<string> { _localizer["changeUsername:nameAlreadyUsed"]}

            };
        }
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        var res = await _userManager.SetUserNameAsync(user, newUserName);
        
        return new ChangeUserNameResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Localize(_localizer)
        };
    }
}

public interface IChangeUserNameService
{
    Task<ChangeUserNameResultModel> ChangeUserNameAsync(string userId, string newUserName);
}

[ExcludeFromCodeCoverage]
public class ChangeUserNameModel
{
    public string NewUserName { get; set; } = string.Empty;
}

public class ChangeUserNameResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}