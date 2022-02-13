// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Text;
using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.Services.Manage;
using Nuages.Identity.UI.AWS;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

[Authorize]
public class EnableAuthenticatorModel : PageModel
{
    private readonly IMFAService _mfaService;
    private readonly NuagesUserManager _userManager;

    public EnableAuthenticatorModel(
        NuagesUserManager userManager,
        IMFAService mfaService)
    {
        _userManager = userManager;
        _mfaService = mfaService;
    }

    public string SharedKey { get; set; }
    public string AuthenticatorUri { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            var url = await _mfaService.GetMFAUrlAsync(user!.Id);

            SharedKey = FormatKey(url.Key);
            AuthenticatorUri = url.Url;

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

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        if (currentPosition < unformattedKey.Length) result.Append(unformattedKey.AsSpan(currentPosition));

        return result.ToString().ToLowerInvariant();
    }
}