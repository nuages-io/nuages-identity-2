using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Nuages.Identity.UI.Tests;

public class BasicPageTestsUser
    : IClassFixture<CustomWebApplicationFactory<Startup>>
{
    private readonly CustomWebApplicationFactory<Startup>  _factory;

    public BasicPageTestsUser(CustomWebApplicationFactory<Startup>  factory)
    {
        _factory = factory;

    }

    [Theory]
    [InlineData("/account/manage/profile")]
    [InlineData("/account/manage/changePassword")]
    [InlineData("/account/manage/email")]
    [InlineData("/account/manage/username")]
    [InlineData("/account/manage/phone")]
    [InlineData("/account/manage/twofactorauthentication")]
    [InlineData("/account/manage/externalLogins")]
    [InlineData("/account/manage/enableAuthenticator")]
    [InlineData("/connect/verify")]
    [InlineData("/connect/verifyDone")]
    public async Task Get_SecurePageIsReturnedForAnAuthenticatedUser(string url)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Test");
        
        //Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/account/manage/setPassword")]
    public async Task Get_SecurePageIsRedirectForAnAuthenticatedUser(string url)
    {
        var client = _factory.CreateClient( new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Test");

        //Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}