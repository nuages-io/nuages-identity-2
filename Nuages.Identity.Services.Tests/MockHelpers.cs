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
// ReSharper disable UnusedParameter.Local

namespace Nuages.Identity.Services.Tests;

public static class MockHelpers
{
    public class MockIdentity
    {
        public NuagesUserManager UserManager { get; set; } = null!;
        public Mock<IUserStore<NuagesApplicationUser>> UserStore { get; set; } = null!;
        public Mock<IUserEmailStore<NuagesApplicationUser>> UserEmaiLStore { get; set; } = null!;
        public Mock<IUserPasswordStore<NuagesApplicationUser>> UserPasswordStore { get; set; } = null!;
        public Mock<IUserLockoutStore<NuagesApplicationUser>> UserLockoutStore { get; set; } = null!;
        public Mock<IUserTwoFactorRecoveryCodeStore<NuagesApplicationUser>> UserRecoveryCodeStore { get; set; } = null!;
        public NuagesIdentityOptions NuagesOptions { get; set; } = new ();
        public FakeSignInManager SignInManager { get; set; }  = null!;
        public Mock<IUserPhoneNumberStore<NuagesApplicationUser>> UserPhoneNumberStore { get; set; } = null!;
    }
    
    public static MockIdentity MockIdentityStuff(NuagesApplicationUser? user, NuagesIdentityOptions? nuagesOptions = null )
    {
        var mockIdentity = new MockIdentity
        {
            UserStore = new Mock<IUserStore<NuagesApplicationUser>>()
        };

        if (nuagesOptions != null)
            mockIdentity.NuagesOptions = nuagesOptions;
        
        mockIdentity.UserEmaiLStore = mockIdentity.UserStore.As<IUserEmailStore<NuagesApplicationUser>>();
        mockIdentity.UserPasswordStore = mockIdentity.UserStore.As<IUserPasswordStore<NuagesApplicationUser>>();
        mockIdentity.UserLockoutStore =  mockIdentity.UserStore.As<IUserLockoutStore<NuagesApplicationUser>>();
        mockIdentity.UserRecoveryCodeStore = mockIdentity.UserStore.As<IUserTwoFactorRecoveryCodeStore<NuagesApplicationUser>>();
        mockIdentity.UserPhoneNumberStore = mockIdentity.UserStore.As<IUserPhoneNumberStore<NuagesApplicationUser>>();
        
        if (user != null)
        {
            mockIdentity.UserStore.Setup(u => u.FindByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync( () => user );
            mockIdentity.UserStore.Setup(u => u.UpdateAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync( () => IdentityResult.Success );
        
            mockIdentity.UserEmaiLStore.Setup(u => u.FindByEmailAsync(user.NormalizedEmail, It.IsAny<CancellationToken>())).ReturnsAsync( () => user);
            mockIdentity.UserEmaiLStore.Setup(u => u.GetEmailConfirmedAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync( () => user.EmailConfirmed);

            mockIdentity.UserPasswordStore.Setup(p => p.GetPasswordHashAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => user.PasswordHash);
            
            mockIdentity.UserPasswordStore.Setup(p => p.HasPasswordAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => !string.IsNullOrEmpty(user.PasswordHash));

            mockIdentity.UserLockoutStore.Setup(u => u.GetLockoutEnabledAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => user.LockoutEnabled);
            
            mockIdentity.UserLockoutStore.Setup(u => u.GetLockoutEndDateAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => user.LockoutEnd);
            
            mockIdentity.UserLockoutStore.Setup(u => u.GetAccessFailedCountAsync(user, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => user.AccessFailedCount);
            
                
            mockIdentity.UserLockoutStore.Setup(u => u.IncrementAccessFailedCountAsync(user, It.IsAny<CancellationToken>()))
                .Callback( (NuagesApplicationUser user2, CancellationToken cancellationToken) => ++user2.AccessFailedCount)
                .ReturnsAsync(() => user.AccessFailedCount);
            
            mockIdentity.UserLockoutStore.Setup(u => u.SetLockoutEndDateAsync(user, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .Callback((NuagesApplicationUser user2, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken) => user2.LockoutEnd = lockoutEnd);
            
            mockIdentity.UserLockoutStore.Setup(u => u.ResetAccessFailedCountAsync(user,  It.IsAny<CancellationToken>()))
                .Callback((NuagesApplicationUser user2,  CancellationToken cancellationToken) => user2.AccessFailedCount = 0);

            mockIdentity.UserRecoveryCodeStore
                .Setup(r => r.RedeemCodeAsync(user, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => true);
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

        nuagesOptionsMock.Setup(o => o.Value).Returns(mockIdentity.NuagesOptions);
      
        
        var userValidators = new List<IUserValidator<NuagesApplicationUser>>();
        var validator = new Mock<IUserValidator<NuagesApplicationUser>>();
        userValidators.Add(validator.Object);
        var pwdValidators = new List<PasswordValidator<NuagesApplicationUser>> { new () };
            
        mockIdentity.UserManager = new NuagesUserManager(mockIdentity.UserStore.Object , options.Object, new PasswordHasher<NuagesApplicationUser>(),
            userValidators, pwdValidators, MockLookupNormalizer(),
            new IdentityErrorDescriber(), null!,
            new Mock<ILogger<NuagesUserManager>>().Object, nuagesOptionsMock.Object);

        mockIdentity.UserManager.Options.SignIn.RequireConfirmedEmail = mockIdentity.NuagesOptions.RequireConfirmedEmail;

        mockIdentity.SignInManager = new FakeSignInManager(mockIdentity.UserManager, Options.Create(mockIdentity.NuagesOptions), null, user);
        
        var token = Guid.NewGuid().ToString();

        var twoFactorTokenProvider = new Mock<IUserTwoFactorTokenProvider<NuagesApplicationUser>>();

        twoFactorTokenProvider.Setup(c =>
                c.GenerateAsync(It.IsAny<string>(), mockIdentity.UserManager, It.IsAny<NuagesApplicationUser>()))
            .ReturnsAsync(() => token);
        twoFactorTokenProvider.Setup(c =>
                c.ValidateAsync(It.IsAny<string>(), token, mockIdentity.UserManager, It.IsAny<NuagesApplicationUser>()))
            .ReturnsAsync(() => true);
        mockIdentity.UserManager.RegisterTokenProvider("Default", twoFactorTokenProvider.Object);
        mockIdentity.UserManager.RegisterTokenProvider("Phone", twoFactorTokenProvider.Object);
        
        validator.Setup(v => v.ValidateAsync(mockIdentity.UserManager, It.IsAny<NuagesApplicationUser>()))
            .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
        
        return mockIdentity;
    }

    public static NuagesApplicationUser CreateDefaultUser(string email = "test@nuages.org")
    {
        return new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email.ToUpper(),
            UserName = email,
            NormalizedUserName = email.ToUpper(),
            EmailConfirmed = true
        };
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
