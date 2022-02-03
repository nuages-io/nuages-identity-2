// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Nuages.AspNetIdentity;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.Pages.Account;

[AllowAnonymous]
public class ExternalLoginModel : PageModel
{
    private readonly NuagesSignInManager _signInManager;
    private readonly NuagesUserManager _userManager;
    private readonly ILogger<ExternalLoginModel> _logger;
    private readonly IStringLocalizer _localizer;
    private readonly UIOptions _options;

    public ExternalLoginModel(NuagesSignInManager signInManager, NuagesUserManager userManager, IUserStore<NuagesApplicationUser> userStore,
        ILogger<ExternalLoginModel> logger, IOptions<UIOptions> options, IStringLocalizer localizer)
    {
        _signInManager = signInManager;
        _userManager = userManager;
  
        _logger = logger;
        _localizer = localizer;
        _options = options.Value;
    }

    public string ProviderDisplayName { get; set; }
    public string ReturnUrl { get; set; }
    public string ErrorMessage { get; set; }
    public string Email { get; set; }
    
    public IActionResult OnGet() => RedirectToPage("./Login");

    public IActionResult OnPost(string provider, string returnUrl = null)
    {
        var redirectUrl = Url.Page("./ExternalLogin",  "Callback",  new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }
    


    public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
    {
        returnUrl ??= Url.Content("~/");
        
        if (remoteError != null)
        {
            _logger.LogError("External Login Error : " + remoteError);
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }
        
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogInformation("External Login Not Found ");
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,  _options.ExternalLoginPersistent,  _options.Bypass2FAWithExternalLogin);
        if (result.Succeeded)
        {
            _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity!.Name, info.LoginProvider);
            return LocalRedirect(returnUrl);
        }
        
        if (result.IsLockedOut)
        {
            ErrorMessage = _localizer["externalLogin:lockout"];
            return Page();
        }
        
        ReturnUrl = returnUrl;
        ProviderDisplayName = info.ProviderDisplayName;
        
        
        if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email) && _options.ExternalLoginAutoEnrollIfEmailExists)
        {
            Email = info.Principal.FindFirstValue(ClaimTypes.Email);
        
            var user = await _userManager.FindByEmailAsync(Email);
            if (user != null)
            {
                if (result.IsNotAllowed)
                {
                    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                    switch (user.LastFailedLoginReason)
                    {
                        case FailedLoginReason.EmailNotConfirmed:
                        {
                            return RedirectToPage("./EmailNotConfirmed");
                        }
                        case FailedLoginReason.PhoneNotConfirmed:
                        {
                            return RedirectToPage("./PhoneNotConfirmed");
                        }
                        case FailedLoginReason.NotWithinDateRange:
                        {
                            ErrorMessage = _localizer[$"errorMessage:no_access:{user.LastFailedLoginReason}"];
                            return Page();
                        }
                        default:
                        {
                            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                        }
                    }
                }
        
                var res = await _userManager.AddLoginAsync(user, info);
                if (res.Succeeded)
                {
                    _logger.LogInformation("{Name} auto enrolled and logged in with {LoginProvider} provider.", info.Principal.Identity!.Name, info.LoginProvider);
                    await _signInManager.SignInAsync(user, _options.ExternalLoginPersistent, info.LoginProvider);
                    return LocalRedirect(returnUrl);
                }
            }
         
        }
        else
        {
            ErrorMessage = _localizer["externalLogin:emailNotAvailable"];
        }
        
        return Page();
    }
}