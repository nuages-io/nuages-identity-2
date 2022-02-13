// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.Services.Manage;
using Nuages.Identity.UI.AWS;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

[Authorize]
public class TwoFactorAuthenticationModel : PageModel
{
    private readonly IMFAService _mfaService;
    private readonly NuagesSignInManager _signInManager;
    private readonly NuagesUserManager _userManager;

    public TwoFactorAuthenticationModel(
        NuagesUserManager userManager, NuagesSignInManager signInManager, IMFAService mfaService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _mfaService = mfaService;
    }

    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public bool Is2FaEnabled { get; set; }
    public bool IsMachineRemembered { get; set; }
    public string RecoveryCodesString { get; set; }

    public List<string> RecoveryCodes { get; set; } = new();
    public string FallbackNumber { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
       
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
            Is2FaEnabled = await _userManager.GetTwoFactorEnabledAsync(user) && HasAuthenticator;

            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

            RecoveryCodes = await _mfaService.GetRecoveryCodes(user.Id);
            RecoveryCodesString = RecoveryCodes.Any() ? string.Join(",", RecoveryCodes) : "";

            if (user.PhoneNumberConfirmed) FallbackNumber = user.PhoneNumber;

            return Page();

        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);

            throw;
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }

    }
}