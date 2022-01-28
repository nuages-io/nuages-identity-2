using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;

namespace Nuages.Identity.Services.Tests;

public static class MockHelpers
{
    public class MockIdentity
    {
        public NuagesUserManager UserManager { get; set; } = null!;
        public Mock<IUserStore<NuagesApplicationUser>> UserStore { get; set; } = null!;
        public Mock<IUserEmailStore<NuagesApplicationUser>> UserEmaiLStore { get; set; } = null!;
        public Mock<IUserPasswordStore<NuagesApplicationUser>> UserPasswordStore { get; set; } = null!;
    }
    public static MockIdentity MockIdentityStuff(NuagesApplicationUser? user, NuagesIdentityOptions? nuagesOptions = null )
    {
        var mockIdentity = new MockIdentity
        {
            UserStore = new Mock<IUserStore<NuagesApplicationUser>>()
        };

        mockIdentity.UserEmaiLStore = mockIdentity.UserStore.As<IUserEmailStore<NuagesApplicationUser>>();
        mockIdentity.UserPasswordStore = mockIdentity.UserStore.As<IUserPasswordStore<NuagesApplicationUser>>();
        
        if (user != null)
        {
            mockIdentity.UserStore.Setup(u => u.FindByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync( () => user );
            mockIdentity.UserStore.Setup(u => u.UpdateAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync( () => IdentityResult.Success );
        
            mockIdentity.UserEmaiLStore.Setup(u => u.FindByEmailAsync(user.NormalizedEmail, It.IsAny<CancellationToken>())).ReturnsAsync( () => user);
            mockIdentity.UserEmaiLStore.Setup(u => u.GetEmailConfirmedAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync( () => user.EmailConfirmed);

            mockIdentity.UserPasswordStore.Setup(p => p.GetPasswordHashAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => user.PasswordHash);
        }
      
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
        
        nuagesOptions ??= new NuagesIdentityOptions();
        
        nuagesOptionsMock.Setup(o => o.Value).Returns(nuagesOptions);
      
        
        var userValidators = new List<IUserValidator<NuagesApplicationUser>>();
        var validator = new Mock<IUserValidator<NuagesApplicationUser>>();
        userValidators.Add(validator.Object);
        var pwdValidators = new List<PasswordValidator<NuagesApplicationUser>> { new () };
            
        mockIdentity.UserManager = new NuagesUserManager(mockIdentity.UserStore.Object , options.Object, new PasswordHasher<NuagesApplicationUser>(),
            userValidators, pwdValidators, MockLookupNormalizer(),
            new IdentityErrorDescriber(), null!,
            new Mock<ILogger<NuagesUserManager>>().Object, nuagesOptionsMock.Object);

        mockIdentity.UserManager.Options.SignIn.RequireConfirmedEmail = nuagesOptions.RequireConfirmedEmail;
       
        var token = Guid.NewGuid().ToString();

        var twoFactorTokenProvider = new Mock<IUserTwoFactorTokenProvider<NuagesApplicationUser>>();

        twoFactorTokenProvider.Setup(c =>
                c.GenerateAsync(It.IsAny<string>(), mockIdentity.UserManager, It.IsAny<NuagesApplicationUser>()))
            .ReturnsAsync(() => token);
        twoFactorTokenProvider.Setup(c =>
                c.ValidateAsync(It.IsAny<string>(), token, mockIdentity.UserManager, It.IsAny<NuagesApplicationUser>()))
            .ReturnsAsync(() => true);
        mockIdentity.UserManager.RegisterTokenProvider("Default", twoFactorTokenProvider.Object);
     
        validator.Setup(v => v.ValidateAsync(mockIdentity.UserManager, It.IsAny<NuagesApplicationUser>()))
            .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
        
        return mockIdentity;
    }



    private static ILookupNormalizer MockLookupNormalizer()
    {
        var normalizerFunc = new Func<string?, string>(i =>
        {
           
            // ReSharper disable once ConvertIfStatementToReturnStatement
            // ReSharper disable once UseNullPropagation
            if (i == null)
            {
                return null!;
            }

            return i.ToUpperInvariant();
        });
        var lookupNormalizer = new Mock<ILookupNormalizer>();
        lookupNormalizer.Setup(i => i.NormalizeName(It.IsAny<string>())).Returns(normalizerFunc);
        lookupNormalizer.Setup(i => i.NormalizeEmail(It.IsAny<string>())).Returns(normalizerFunc);
        return lookupNormalizer.Object;
    }


    public static Mock<IHttpContextAccessor> MockHttpContextAccessor()
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
}

public class FakeStringLocalizer : IStringLocalizer
{
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        throw new NotImplementedException();
    }

    public LocalizedString this[string name] => new (name, name);

    public LocalizedString this[string name, params object[] arguments] => new (name, name);
    
}
