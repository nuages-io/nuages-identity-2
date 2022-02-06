using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nuages.Identity.UI.Tests;

public class TestAuthHandler : AuthenticationHandler<TestAuthHandlerOptions>
{
    public TestAuthHandler(IOptionsMonitor<TestAuthHandlerOptions> options, 
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            //new Claim(ClaimTypes.NameIdentifier,  Options.DefaultUserId),
            new Claim("sub", Options.DefaultUserId), 
            //new Claim("name", $"{Options.DefaultUserId}@nuages.org"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}