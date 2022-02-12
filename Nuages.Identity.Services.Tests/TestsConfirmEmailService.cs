using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Nuages.Identity.Services.Email;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsConfirmEmailService
{
    [Fact]
    public async Task ConfirmEmailWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();

        var mockUserManager = MockHelpers.MockIdentityStuff(user).UserManager;

        var code = await mockUserManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var confirmEmailService = new ConfirmEmailService(mockUserManager);

        Assert.True(await confirmEmailService.Confirm(user.Id, code));
    }

    [Fact]
    public async Task ConfirmEmailWithError()
    {
        var user = MockHelpers.CreateDefaultUser();

        var mockUserManager = MockHelpers.MockIdentityStuff(user).UserManager;

        var confirmEmailService = new ConfirmEmailService(mockUserManager);

        Assert.False(await confirmEmailService.Confirm(MockHelpers.BadId, "code"));
    }
}