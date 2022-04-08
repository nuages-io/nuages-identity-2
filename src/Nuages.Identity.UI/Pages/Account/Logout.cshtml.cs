// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.UI.AWS;

// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;
    private readonly NuagesSignInManager _signInManager;

    public LogoutModel(NuagesSignInManager signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet(string returnUrl = null)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            
            await _signInManager.SignOutAsync();
            
            _logger.LogInformation("YO! User logged out.");
            _logger.LogInformation($"LocalRedirect returnUrl = {returnUrl}");
            if (returnUrl != null) return Redirect(returnUrl);
            _logger.LogInformation($"Redirect to root");
            return Redirect("/");
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