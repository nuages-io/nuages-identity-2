using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Nuages.Identity.UI.Tests;


public class BasicPageTests 
    : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly HttpClient _authenticatedClient;

    public BasicPageTests(WebApplicationFactory<Startup> factory)
    {
        _factory = factory;
        
        // Arrange
        _authenticatedClient = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = "Test";
                            options.DefaultChallengeScheme = "Test";
                            options.DefaultScheme = "Test";
                        })
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test", options => {});
                });
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

        _authenticatedClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Test");

    }

    [Theory]
    [InlineData("/")]
    [InlineData("/account/login")]
    [InlineData("/account/forgotpassword")]
    [InlineData("/account/register")]
    [InlineData("/account/loginWithPasswordless")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
        Assert.Equal("text/html; charset=utf-8", 
            response.Content.Headers.ContentType.ToString());
    }
    
    [Theory]
    [InlineData("/account/manage/profile")]
    [InlineData("/account/manage/changePassword")]
    [InlineData("/account/manage/email")]
    [InlineData("/account/manage/username")]
    [InlineData("/account/manage/twofactorauthentication")]
    [InlineData("/account/manage/externalLogins")]
    public async Task Get_SecurePageIsReturnedForAnAuthenticatedUser2(string url)
    {

        //Act
        var response = await _authenticatedClient.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    
}