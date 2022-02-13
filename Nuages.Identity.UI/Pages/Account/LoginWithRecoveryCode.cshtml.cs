// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.UI.AWS;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

public class LoginWithRecoveryCodeModel : PageModel
{
    private readonly NuagesSignInManager _signInManager;

    public LoginWithRecoveryCodeModel(
        NuagesSignInManager signInManager)
    {
        _signInManager = signInManager;
    }


    public string ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(string returnUrl = null)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) throw new InvalidOperationException("Unable to load two-factor authentication user.");

            ReturnUrl = returnUrl ?? Url.Content("~/");

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