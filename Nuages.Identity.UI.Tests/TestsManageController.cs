using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.Services.Manage;
using OtpNet;
using Xunit;

namespace Nuages.Identity.UI.Tests;

public class TestsManageController : IClassFixture<CustomWebApplicationFactory<Startup>>
{
    private readonly CustomWebApplicationFactory<Startup> _factory;
    private readonly JsonSerializerOptions _options;
    private readonly NuagesUserManager _userManager;
    
    public TestsManageController(CustomWebApplicationFactory<Startup> factory)
    {
        _factory = factory;
        
        var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        var scope = scopeFactory.CreateScope();

        _userManager = scope.ServiceProvider.GetRequiredService<NuagesUserManager>();
        
        _options  = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        _options.Converters.Add(new JsonStringEnumConverter());
    }

    [Fact]
    public async Task ShouldSaveProfileWithSuccess()
    {
        var client = _factory.CreateClient();

        var body = new SaveProfileModel
        {
            LastName = "LastName",
            FirstName = "FirsName"
        };

        var res = await client.PostAsync("api/manage/saveProfile", 
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        var content = await res.Content.ReadAsStringAsync();
        
        var response = JsonSerializer.Deserialize<SaveProfileResultModel>(content, _options);
        
        Assert.True(response!.Success);
    }
    
    [Fact]
    public async Task ShouldChange2FaEnabledState()
    {
        var userId = IdentityDataSeeder.UserId_MFA;
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserId, userId);
        
        var body = new SaveProfileModel
        {
            LastName = "LastName",
            FirstName = "FirsName"
        };

        var res = await client.DeleteAsync("api/manage/disable2Fa");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        var content = await res.Content.ReadAsStringAsync();
        
        var response = JsonSerializer.Deserialize<DisableMFAResultModel>(content, _options);
        
        Assert.True(response!.Success);

        var user = await _userManager.FindByIdAsync(userId);
        
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        
        var totp = new Totp(Base32Encoding.ToBytes(key)).ComputeTotp();
        
        var bodyEnable = new EnableMFAModel
        {
            Code = totp
        };
        
        res = await client.PostAsync("api/manage/enable2Fa", 
            new StringContent(JsonSerializer.Serialize(bodyEnable), System.Text.Encoding.UTF8, "application/json"));
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        content = await res.Content.ReadAsStringAsync();
        
        var responseEnable = JsonSerializer.Deserialize<MFAResultModel>(content, _options);
        
        Assert.True(responseEnable!.Success);
    }
}