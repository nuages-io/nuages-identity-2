using System.Net;
using System.Text.Json;
using Xunit;

namespace Nuages.Identity.UI.Tests;

public class TestsAccountControllerAnonymous : IClassFixture<CustomWebApplicationFactoryAnonymous<Startup>>
{
    private readonly CustomWebApplicationFactoryAnonymous<Startup> _factory;

    public TestsAccountControllerAnonymous(CustomWebApplicationFactoryAnonymous<Startup> factory)
    {
        _factory = factory;
        
    }

    [Fact]
    public async Task ShouldLoginWithSuccess()
    {
        var client = _factory.CreateClient();

        var body = new
        {
            UserNameOrEmail = IdentityDataSeeder.UserEmail,
            Password = IdentityDataSeeder.UserPassword
        };

        var res = await client.PostAsync("api/account/login", new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        var content = await res.Content.ReadAsStringAsync();
        
        var success = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = true })!.success;
        Assert.True(success);
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
    
    [Fact]
    public async Task ShouldLForgotPasswordWithSuccess()
    {
        var client = _factory.CreateClient();

        var body = new
        {
            Email = IdentityDataSeeder.UserEmail
        };

        var res = await client.PostAsync("api/account/forgotPassword", new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        var content = await res.Content.ReadAsStringAsync();
        
        var success = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = true })!.success;
        
        Assert.True(success);
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}