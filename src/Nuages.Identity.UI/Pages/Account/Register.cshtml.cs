// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.UI.Setup;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly NuagesSignInManager _signInManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly UIOptions _options;

    public RegisterModel(NuagesSignInManager signInManager, ILogger<RegisterModel> logger, IOptions<UIOptions> options )
    {
        _signInManager = signInManager;
        _logger = logger;
        _options = options.Value;
    }

    public string ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public async Task<IActionResult> OnGetAsync(string returnUrl = null)
    {
        try
        {

            if (!_options.AllowSelfRegistration)
            {
                return NotFound();
            }
            
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).Where(l => l.Name != "JwtOrCookie").ToList();

            return Page();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }
}