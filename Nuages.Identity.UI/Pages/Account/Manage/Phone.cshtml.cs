// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

[Authorize]
public class PhoneModel : PageModel
{
    [TempData] 
    public string PhoneNumber { get; set; } = string.Empty;

    public void OnGetAsync()
    {
        if (System.IO.File.Exists(@"wwwroot/data/countryCodes.json"))
        {
            using var file = System.IO.File.OpenText(@"wwwroot/data/countryCodes.json");
            Codes = JsonSerializer.Deserialize<CountryCode[]>(file.BaseStream);
        }
        else
        {
            Codes = new[]
            {
                new CountryCode
                {
                    name = "default",
                    dial_code = "1"
                }
            };
        }
    }

    public CountryCode[] Codes { get; set; } = Array.Empty<CountryCode>();
}

// ReSharper disable once ClassNeverInstantiated.Global
public class CountryCode
{
    // ReSharper disable once InconsistentNaming
    public string name { get; set; }
    // ReSharper disable once InconsistentNaming
    public string dial_code { get; set; }

    
}