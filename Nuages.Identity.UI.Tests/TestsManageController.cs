using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.Services.Manage;
using OtpNet;
using Xunit;

namespace Nuages.Identity.UI.Tests;

[Collection("IntegrationTestUI")]
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
        const string userId = IdentityDataSeeder.UserId_MFA;
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserId, userId);
        
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
    
    
    [Fact]
    public async Task ShouldDownloadRecoveryCodes()
    {
        var client = _factory.CreateClient();


        var res = await client.GetAsync("api/manage/downloadRecoveryCodes");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        Assert.Equal("application/text",
            res.Content.Headers.ContentType!.ToString());
    }
    
    [Fact]
    public async Task ShouldResetRecoveryCodes()
    {
        var client = _factory.CreateClient();
        
        var res = await client.PostAsync("api/manage/resetRecoveryCodes", 
            null);
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        var content = await res.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<MFAResultModel>(content, _options);
        
        Assert.True(response!.Success);
        Assert.NotEmpty(response.Codes);
    }
    
    [Fact]
    public async Task ShouldForgetBrowsers()
    {
        var client = _factory.CreateClient();
        
        var res = await client.PostAsync("api/manage/forgetBrowser", 
            null);
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        var content = await res.Content.ReadAsStringAsync();
        var responseEnable = JsonSerializer.Deserialize<bool>(content, _options);
        
        Assert.True(responseEnable);
    }
    
    [Fact]
    public async Task ShouldChangeUserNameWithSuccess()
    {
        var client = _factory.CreateClient();

        var user = await _userManager.FindByIdAsync(_factory.DefaultUserId);
        var currentUserName = user.UserName;
        var body = new ChangeUserNameModel
        {
            NewUserName = currentUserName + "2"
        };

        var res = await client.PostAsync("api/manage/changeUserName", 
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        var content = await res.Content.ReadAsStringAsync();
        
        var response = JsonSerializer.Deserialize<ChangeUserNameResultModel>(content, _options);
        
        Assert.True(response!.Success);
        
        
        body = new ChangeUserNameModel
        {
            NewUserName = currentUserName 
        };

        res = await client.PostAsync("api/manage/changeUserName", 
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        content = await res.Content.ReadAsStringAsync();
        
        response = JsonSerializer.Deserialize<ChangeUserNameResultModel>(content, _options);
        
        Assert.True(response!.Success);
    }
    
    [Fact]
    public async Task ShouldChangeAndRemovePhoneWithSuccess()
    {
        var client = _factory.CreateClient();

        var body2 = new SendSMSVerificationCodeModel
        {
            PhoneNumber = "8888888888"
        };

        var res = await client.PostAsync("api/manage/sendPhoneChangeMessage",
            new StringContent(JsonSerializer.Serialize(body2), System.Text.Encoding.UTF8, "application/json"));
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        var content = await res.Content.ReadAsStringAsync();
        
        var response = JsonSerializer.Deserialize<SendSMSVerificationCodeResultModel>(content, _options);
        
        Assert.True(response!.Success);
        
        
        var body = new ChangePhoneNumberModel
        {
            PhoneNumber = body2.PhoneNumber,
            Token = response.Code!
        };
        res = await client.PostAsync("api/manage/changePhoneNumber", 
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        content = await res.Content.ReadAsStringAsync();
        
        var response2 = JsonSerializer.Deserialize<ChangePhoneNumberResultModel>(content, _options);
        
        Assert.True(response2!.Success);
        
        
        res = await client.DeleteAsync("api/manage/removePhone");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        content = await res.Content.ReadAsStringAsync();
        
        var response3 = JsonSerializer.Deserialize<ChangePhoneNumberResultModel>(content, _options);
        
        Assert.True(response3!.Success);


    }
    
    [Fact]
    public async Task ShouldChangePasswordWithSuccess()
    {
        var client = _factory.CreateClient();

        var body = new ChangePasswordModel
        {
            CurrentPassword = IdentityDataSeeder.UserPassword,
            NewPassword = IdentityDataSeeder.UserPassword,
            NewPasswordConfirm = IdentityDataSeeder.UserPassword
        };

        var res = await client.PostAsync("api/manage/changePassword", 
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        var content = await res.Content.ReadAsStringAsync();
        
        var response = JsonSerializer.Deserialize<ChangePasswordResultModel>(content, _options);
        
        Assert.True(response!.Success);

        var user = await _userManager.FindByIdAsync(_factory.DefaultUserId);
        await _userManager.RemovePasswordAsync(user);
        
        var body2 = new ChangePasswordModel
        {
            NewPassword = IdentityDataSeeder.UserPassword,
            NewPasswordConfirm = IdentityDataSeeder.UserPassword
        };
        
        res = await client.PostAsync("api/manage/setPassword", 
            new StringContent(JsonSerializer.Serialize(body2), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        content = await res.Content.ReadAsStringAsync();
        
        response = JsonSerializer.Deserialize<ChangePasswordResultModel>(content, _options);
        
        Assert.True(response!.Success);
    }

    [Fact]
    public async Task ShouldInitiateChangeEmailWithSuccess()
    {
        var client = _factory.CreateClient();

        var body = new SendEmailChangeModel
        {
            Email = "newemail@example.com"
        };
        
        var res = await client.PostAsync("api/manage/sendEmailChange", 
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        var content = await res.Content.ReadAsStringAsync();
        
        var response = JsonSerializer.Deserialize<SendEmailChangeResultModel>(content, _options);
        
        Assert.True(response!.Success);

        var res2 = await client.GetAsync(response.Url);
        Assert.Equal(HttpStatusCode.OK, res2.StatusCode);
        
    }
}