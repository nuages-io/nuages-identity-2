using Xunit;

namespace Nuages.Identity.UI.Tests;

public class TestsBasicPageUserAnonymous
    : IClassFixture<CustomWebApplicationFactoryAnonymous<Program>>
{
    private readonly CustomWebApplicationFactoryAnonymous<Program> _factory;

    public TestsBasicPageUserAnonymous(CustomWebApplicationFactoryAnonymous<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Index")]
    [InlineData("/account/login")]
    [InlineData("/account/forgotpassword")]
    [InlineData("/account/register")]
    [InlineData("/account/loginWithMagicLink")]
    [InlineData("/account/emailNotConfirmed")]
    [InlineData("/account/logout")]
    [InlineData("/error")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("text/html; charset=utf-8",
            response.Content.Headers.ContentType!.ToString());
    }
}