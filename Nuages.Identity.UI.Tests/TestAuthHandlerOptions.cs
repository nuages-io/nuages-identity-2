using Microsoft.AspNetCore.Authentication;

namespace Nuages.Identity.UI.Tests;

public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public string DefaultUserId { get; set; } = null!;
}