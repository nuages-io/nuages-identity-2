using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
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
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("UserNotFound");

        if (string.IsNullOrEmpty(token))
            token = await _userManager.GenerateChangeEmailTokenAsync(user, email);
        
        var res = await _userManager.ChangeEmailAsync(user, email, token);

        return new ChangeEmailResultModel
        {
            Success = res.Succeeded,
            Errors = res.Errors.Select(e => _localizer[$"identity.{e.Code}"].Value).ToList()
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

public class ChangeEmailModel
{
    public string Email { get; set; } = string.Empty;
}