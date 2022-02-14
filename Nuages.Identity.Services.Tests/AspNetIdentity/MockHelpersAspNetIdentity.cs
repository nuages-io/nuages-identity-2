using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;

// ReSharper disable UnusedParameter.Local

namespace Nuages.Identity.Services.Tests.AspNetIdentity;

public static class MockHelpersAspNetIdentity
{
    public const string StrongPassword = "ThisIsAStrongPassword789*$#$$$";
    public const string WeakPassword = "password";
    public const string PhoneNumber = "9999999999";

    public static MockIdentity MockIdentityStuff(NuagesIdentityOptions? nuagesOptions = null)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddDbContext<TestDataContext>(options => { options.UseInMemoryDatabase("IdentityContext"); });

        var identityBuilder = new IdentityBuilder(typeof(NuagesApplicationUser<string>),
            typeof(NuagesApplicationRole<string>), serviceCollection);
        identityBuilder.AddEntityFrameworkStores<TestDataContext>();


        var serviceProvider = serviceCollection.BuildServiceProvider();


        var context = serviceProvider.GetRequiredService<TestDataContext>();
        var inMemoryStorage = serviceProvider.GetRequiredService<IUserStore<NuagesApplicationUser<string>>>();

        var mockIdentity = new MockIdentity
        {
            DataContext = context
            //UserStore = new InMemoryUserStore<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>(inMemoryStorage)
        };

        if (nuagesOptions != null)
            mockIdentity.NuagesOptions = nuagesOptions;

        var options = new Mock<IOptions<IdentityOptions>>();
        var idOptions = new IdentityOptions
        {
            Lockout =
            {
                AllowedForNewUsers = false
            }
        };

        options.Setup(o => o.Value).Returns(idOptions);

        var nuagesOptionsMock = new Mock<IOptions<NuagesIdentityOptions>>();

        nuagesOptionsMock.Setup(o => o.Value).Returns(mockIdentity.NuagesOptions);

        var userValidators = new List<IUserValidator<NuagesApplicationUser<string>>>();
        var validator = new Mock<IUserValidator<NuagesApplicationUser<string>>>();
        userValidators.Add(validator.Object);
        var pwdValidators = new List<PasswordValidator<NuagesApplicationUser<string>>> { new() };

        mockIdentity.UserManager = new NuagesUserManager(inMemoryStorage, options.Object,
            new PasswordHasher<NuagesApplicationUser<string>>(),
            userValidators, pwdValidators, MockLookupNormalizer(),
            new IdentityErrorDescriber(), null!,
            new Mock<ILogger<NuagesUserManager>>().Object, nuagesOptionsMock.Object);

        mockIdentity.UserManager.Options.SignIn.RequireConfirmedEmail = true;

        var mockConfirmation = new Mock<IUserConfirmation<NuagesApplicationUser<string>>>();
        mockConfirmation.Setup(c =>
                c.IsConfirmedAsync(It.IsAny<UserManager<NuagesApplicationUser<string>>>(),
                    It.IsAny<NuagesApplicationUser<string>>()))
            .ReturnsAsync((UserManager<NuagesApplicationUser<string>> _, NuagesApplicationUser<string> user2) =>
                user2.EmailConfirmed);

        mockIdentity.SignInManager = new NuagesSignInManager(mockIdentity.UserManager, MockHttpContextAccessor().Object,
            new Mock<IUserClaimsPrincipalFactory<NuagesApplicationUser<string>>>().Object, Options.Create(idOptions),
            new Mock<ILogger<SignInManager<NuagesApplicationUser<string>>>>().Object
            , new Mock<IAuthenticationSchemeProvider>().Object, mockConfirmation.Object,
            Options.Create(mockIdentity.NuagesOptions));

        var newToken = Guid.NewGuid().ToString();

        var twoFactorTokenProvider = new Mock<IUserTwoFactorTokenProvider<NuagesApplicationUser<string>>>();

        twoFactorTokenProvider.Setup(c =>
                c.GenerateAsync(It.IsAny<string>(), mockIdentity.UserManager,
                    It.IsAny<NuagesApplicationUser<string>>()))
            .ReturnsAsync(() => newToken);

        twoFactorTokenProvider.Setup(c =>
                c.ValidateAsync(It.IsAny<string>(), It.IsAny<string>(), mockIdentity.UserManager,
                    It.IsAny<NuagesApplicationUser<string>>()))
            .ReturnsAsync((string purpose, string token, UserManager<NuagesApplicationUser<string>> manager,
                NuagesApplicationUser<string> _) => token != "bad_token");

        mockIdentity.UserManager.RegisterTokenProvider("Default", twoFactorTokenProvider.Object);
        mockIdentity.UserManager.RegisterTokenProvider("Phone", twoFactorTokenProvider.Object);
        mockIdentity.UserManager.RegisterTokenProvider("PasswordlessLoginProvider", twoFactorTokenProvider.Object);
        mockIdentity.UserManager.RegisterTokenProvider("Authenticator", twoFactorTokenProvider.Object);

        validator.Setup(v => v.ValidateAsync(mockIdentity.UserManager, It.IsAny<NuagesApplicationUser<string>>()))
            .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

        return mockIdentity;
    }

    private static ILookupNormalizer MockLookupNormalizer()
    {
        var normalizerFunc = new Func<string?, string>(i =>
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            // ReSharper disable once UseNullPropagation
            if (i == null) return null!;

            return i.ToUpperInvariant();
        });
        var lookupNormalizer = new Mock<ILookupNormalizer>();
        lookupNormalizer.Setup(i => i.NormalizeName(It.IsAny<string>())).Returns(normalizerFunc);
        lookupNormalizer.Setup(i => i.NormalizeEmail(It.IsAny<string>())).Returns(normalizerFunc);
        return lookupNormalizer.Object;
    }


    private static Mock<IHttpContextAccessor> MockHttpContextAccessor()
    {
        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock
            .Setup(_ => _.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.FromResult((object?)null));


        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(_ => _.GetService(typeof(IAuthenticationService)))
            .Returns(authServiceMock.Object);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext
        {
            RequestServices = serviceProviderMock.Object
        };

        mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

        return mockHttpContextAccessor;
    }

    public class MockIdentity
    {
        //public InMemoryUserStore<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string> UserStore { get; set; } = null!;

        public NuagesIdentityOptions NuagesOptions { get; set; } = new();

        public NuagesUserManager UserManager { get; set; } = null!;
        public NuagesSignInManager SignInManager { get; set; } = null!;

        public TestDataContext DataContext { get; set; } = null!;
    }
}