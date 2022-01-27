using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.Services.Tests;

public class FakeSignInManager : NuagesSignInManager
{
    public FakeSignInManager(NuagesUserManager userManager, IOptions<NuagesIdentityOptions> options, IHttpContextAccessor? contextAccessor = null)
        : base(userManager,
            contextAccessor ?? MockHelpers.MockHttpContextAccessor().Object,
            new Mock<IUserClaimsPrincipalFactory<NuagesApplicationUser>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<NuagesApplicationUser>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<NuagesApplicationUser>>().Object,
            options)
    { }        
}