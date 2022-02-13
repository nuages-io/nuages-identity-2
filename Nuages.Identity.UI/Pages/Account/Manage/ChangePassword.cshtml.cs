// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.UI.AWS;


// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

[Authorize]
public class ChangePasswordModel : PageModel
{
    private readonly NuagesUserManager _userManager;

    public ChangePasswordModel(NuagesUserManager userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword) return RedirectToPage("./SetPassword");

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