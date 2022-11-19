// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Fido2;
using Nuages.Identity.Services.Fido2.Storage;
using Nuages.Identity.Services.Manage;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

[Authorize]
public class TwoFactorAuthenticationModel : PageModel
{
    private readonly IMFAService _mfaService;
    private readonly ILogger<TwoFactorAuthenticationModel> _logger;
    private readonly NuagesSignInManager _signInManager;
    private readonly NuagesUserManager _userManager;
    private readonly IFido2Service? _fido2Service;
    
    public TwoFactorAuthenticationModel(
        NuagesUserManager userManager, NuagesSignInManager signInManager, IMFAService mfaService, 
        IEnumerable<IFido2Service> fido2Service, ILogger<TwoFactorAuthenticationModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _mfaService = mfaService;
        _logger = logger;

        _fido2Service = fido2Service.FirstOrDefault();

    }

    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public bool Is2FaEnabled { get; set; }
    public bool IsMachineRemembered { get; set; }
    public string RecoveryCodesString { get; set; } = null!;
    public string Username { get; set; } = null!;
    
    public List<string> RecoveryCodes { get; set; } = new();
    public string FallbackNumber { get; set; } = null!;

    public bool SecurityKeysEnabled { get; set; }
    public List<IFido2Credential> SecurityKeys { get; set; } = new();
    
    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (_fido2Service != null)
            {
                SecurityKeysEnabled = true;
                SecurityKeys =
                    await _fido2Service.GetSecurityKeysForUser(Encoding.UTF8.GetBytes(user.Id));
            }
            
            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
            
            Is2FaEnabled = await _userManager.GetTwoFactorEnabledAsync(user) && HasAuthenticator;

            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

            RecoveryCodes = await _mfaService.GetRecoveryCodes(user.Id);
            RecoveryCodesString = RecoveryCodes.Any() ? string.Join(",", RecoveryCodes) : "";
            
            if (user.PhoneNumberConfirmed) 
                FallbackNumber = user.PhoneNumber!;

            Username = user.UserName!;
            
            return Page();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
    }
}