using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsNuagesUserManager
{
    [Fact]
    public async Task ShoudGetRecoveryCodesWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var codes = await identityStuff.UserManager.GetRecoveryCodes(user);
        
        Assert.Equal(10, codes.Count);
    }
}