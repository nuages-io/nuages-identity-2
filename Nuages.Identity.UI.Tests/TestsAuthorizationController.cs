using System.Net;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Nuages.Web.Utilities;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Nuages.Identity.UI.Tests;

[Collection("IntegrationTestUI")]
public class TestsAuthorizationController : IClassFixture<CustomWebApplicationFactory<Startup>>
{
    private readonly CustomWebApplicationFactory<Startup> _factory;

    public TestsAuthorizationController(CustomWebApplicationFactory<Startup> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ShouldLoginWithPasswordGrant()
    {
        var httpClient = _factory.CreateClient();

        var res = await httpClient.PostAsync("connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", IdentityDataSeeder.AdminUserName },
            { "password", IdentityDataSeeder.AdminPassword },
            { "audience", "IdentityAPI" },
            { "scope", "email profile openid" },
            { "client_id", "postman-ui" },
            { "client_secret", "511536EF-F270-4058-80CA-1C89C192F69A" }
        }));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<TokenResult>(content)!;

        Assert.NotNull(result.access_token);
        Assert.NotNull(result.id_token);
        Assert.NotNull(result.token_type);
        Assert.Equal("Bearer", result.token_type);
    }

    [Fact]
    public async Task ShouldLoginWithClientCredntialGrant()
    {
        var httpClient = _factory.CreateClient();

        var res = await httpClient.PostAsync("connect/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "audience", "IdentityAPI" },
            { "client_id", "postman-ui" },
            { "client_secret", "511536EF-F270-4058-80CA-1C89C192F69A" }
        }));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<TokenResult>(content)!;

        Assert.NotNull(result.access_token);
        Assert.Null(result.id_token);
        Assert.NotNull(result.token_type);
        Assert.Equal("Bearer", result.token_type);
    }

    [Fact]
    public async Task ShouldAuthorize()
    {
        var httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        const string client_id = "postman-ui";
        const string audience = "IdentityAPI";
        const string scope = "email openid profile";
        const string redirect_uri = "https://oauth.pstmn.io/v1/callback";
        var code_challenge = Pkce.GenerateCodeChallenge(Pkce.GenerateCodeVerifier());

        var url =
            $"connect/authorize?response_type=code&client_id={client_id}&code_challenge={code_challenge}" +
            $"&code_challenge_method=S256&&redirect_uri={redirect_uri}" +
            $"&scope={scope}&audience={audience}";

        var res = await httpClient.GetAsync(url);

        var contents = await res.Content.ReadAsStringAsync();
        Assert.Empty(contents);

        Assert.Equal(HttpStatusCode.Found, res.StatusCode);

        var location = res.Headers.GetValues("Location").FirstOrDefault();
        Assert.NotNull(location);

        var uri = new Uri(location!);

        var parameetrs = HttpUtility.ParseQueryString(uri.Query);

        parameetrs = HttpUtility.ParseQueryString(parameetrs.Get("ReturnUrl")!);

        var value = parameetrs.Get("redirect_uri");
        Assert.Equal(redirect_uri, value);
    }

    [Fact]
    public async Task ShouldLogout()
    {
        var httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var res = await httpClient.GetAsync("connect/logout");

        Assert.Equal(HttpStatusCode.Found, res.StatusCode);

        var contents = await res.Content.ReadAsStringAsync();

        Assert.Empty(contents);
    }

    public class TokenResult
    {
        public string? access_token { get; set; }
        public string? token_type { get; set; }
        public string? id_token { get; set; }
    }
}