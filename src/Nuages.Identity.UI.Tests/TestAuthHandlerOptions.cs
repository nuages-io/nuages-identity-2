using Microsoft.AspNetCore.Authentication;
using Xunit;

namespace Nuages.Identity.UI.Tests;

[Collection("IntegrationTestUI")]
public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    public string DefaultUserId { get; set; } = null!;
}