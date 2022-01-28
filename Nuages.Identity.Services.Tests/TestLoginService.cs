using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Login;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestLoginService
{
    [Fact]
    public async Task ShouldLoginWithSuccess()
    {
        var email = "TEST@NUAGES.ORG";
        var password = "Nuages123*$";
        
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            NormalizedEmail = email,
            UserName = email,
            NormalizedUserName = email,
            EmailConfirmed = true
        };
        
        var options = new NuagesIdentityOptions();

        var identityStuff = MockHelpers.MockIdentityStuff(user, options);

        user.PasswordHash = identityStuff.UserManager.PasswordHasher.HashPassword(user, password);
        
        var fakeSignInManager = new FakeSignInManager(identityStuff.UserManager, Options.Create(options));
        
        identityStuff.UserStore.Setup(u => u.CreateAsync(It.IsAny<NuagesApplicationUser>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>IdentityResult.Success);
        
        var messageService = new Mock<IMessageService>();

        var loginService = new LoginService(identityStuff.UserManager, fakeSignInManager, new FakeStringLocalizer(),
            messageService.Object);

        var res = await loginService.LoginAsync(new LoginModel
        {
            UserNameOrEmail = user.Email,
            Password = password,
            RememberMe = false
        });
        
        Assert.True(res.Success);
    }
}