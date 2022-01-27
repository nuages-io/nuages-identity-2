using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsConfirmEmailService
{
    [Fact]
    public async Task ConfirmEmailWithSuccess()
    {
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString()
        };

        var mockUserManager = MockHelpers.MockIdentityStuff(user).UserManager;
      
        var code = await mockUserManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        var confirmEmailService = new ConfirmEmailService(mockUserManager);

        Assert.True(await confirmEmailService.Confirm(user.Id, code));
    }
    
    [Fact]
    public async Task ConfirmEmailWithError()
    {
        var user = new NuagesApplicationUser
        {
            Id = Guid.NewGuid().ToString()
        };

        var mockUserManager = MockHelpers.MockIdentityStuff(user).UserManager;
      
        var confirmEmailService = new ConfirmEmailService(mockUserManager);

        Assert.False(await confirmEmailService.Confirm("Bad_id", "code"));
    }
}