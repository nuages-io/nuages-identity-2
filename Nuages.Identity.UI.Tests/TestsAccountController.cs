using System.Net;
using System.Text.Json;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.UI.Tests.Utilities;
using OtpNet;
using Xunit;

namespace Nuages.Identity.UI.Tests;

public class TestsAccountControllerAnonymous : IClassFixture<CustomWebApplicationFactoryAnonymous<Startup>>
{
    private readonly CustomWebApplicationFactoryAnonymous<Startup> _factory;
    private readonly NuagesUserManager _userManager;

    public TestsAccountControllerAnonymous(CustomWebApplicationFactoryAnonymous<Startup> factory)
    {
        _factory = factory;
        
        var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        var scope = scopeFactory.CreateScope();

        _userManager = scope.ServiceProvider.GetRequiredService<NuagesUserManager>();
        
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

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        var content = await res.Content.ReadAsStringAsync();
        
        var success = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = true })!.success;
        Assert.True(success);
        
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
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        var content = await res.Content.ReadAsStringAsync();
        
        var result = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = true, url = "", code = "" })!;
        
        Assert.True(result.success);
        
        //Go to reset page
        
        res = await client.GetAsync(result.url);
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var resetBody = new 
        {
            Email = IdentityDataSeeder.UserEmail,
            Password = "Nuages123*",
            PasswordCOnfirm = "Nuages123*",
            Code = result.code
        };
        
        res = await client.PostAsync("api/account/resetPassword", 
            new StringContent(JsonSerializer.Serialize(resetBody), System.Text.Encoding.UTF8, "application/json"));
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        content = await res.Content.ReadAsStringAsync();
        
        var result2 = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = true })!;
        
        Assert.True(result2.success);
    }
    
    [Fact]
    public async Task ShouldStartPasswordlessWithSuccess()
    {
        var client = _factory.CreateClient();

        var body = new
        {
            Email = IdentityDataSeeder.UserEmail
        };

        var res = await client.PostAsync("api/account/passwordlessLogin", 
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        var content = await res.Content.ReadAsStringAsync();
        
        var result = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = true, url = "" });
        
        Assert.True(result!.success);
        Assert.NotNull(result.url);

        res = await client.GetAsync(result.url);
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
    
    [Fact]
    public async Task ShouldLoginWithErrorUnconfirmed()
    {
        var client = _factory.CreateClient();

        var body = new
        {
            UserNameOrEmail = IdentityDataSeeder.UserEmail_Unconfirmed,
            Password = IdentityDataSeeder.UserPassword_Unconfirmed
        };

        //Login
        var res = await client.PostAsync("api/account/login", 
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        var content = await res.Content.ReadAsStringAsync();
        
        var result = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = true, reason = "" })!;
        Assert.False(result.success);
        Assert.Equal("EmailNotConfirmed", result.reason);
        
        //Got to EmailNotCOnfirmed page
        res = await client.GetAsync("/account/emailnotconfirmed");
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        //Send confirmation link

        res = await client.PostAsync("api/account/sendEmailConfirmation",
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        content = await res.Content.ReadAsStringAsync();
        var confirmSuccess = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = true, url = "" })!;
        
        //Confirm email
        res = await client.GetAsync(confirmSuccess.url);
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        //Login again
        
        res = await client.PostAsync("api/account/login", 
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        content = await res.Content.ReadAsStringAsync();
        
        var successLogin = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = true })!;
        Assert.True(successLogin.success);
    }

    [Fact]
    public async Task ShouldLoginWithMfa()
    {
        var client = _factory.CreateClient();

        var body = new
        {
            UserNameOrEmail = IdentityDataSeeder.UserEmail_MFA,
            Password = IdentityDataSeeder.UserPassword_MFA
        };

        //Login
        var res = await client.PostAsync("api/account/login",
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();

        var result = JsonSerializerExtensions.DeserializeAnonymousType(content, 
            new { success = true, 
                result = new
                {
                    requiresTwoFactor =  false
                } 
            })!;
        
        Assert.False(result.success);
        Assert.True(result.result.requiresTwoFactor);

        //Go to MFALogin page
        
        res = await client.GetAsync("/account/loginwith2fa?returnUrl=~/");
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        //Login with Code

        var user = await _userManager.FindByEmailAsync(IdentityDataSeeder.UserEmail_MFA);
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        
        var totp = new Totp(Base32.FromBase32String(key)).ComputeTotp();
        
        var loginBody = new
        {
            code = totp
        };
        
        res = await client.PostAsync("api/account/login2fa",
            new StringContent(JsonSerializer.Serialize(loginBody), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        content = await res.Content.ReadAsStringAsync();
        
        var confirmSuccess = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = false })!;
        
        Assert.True(confirmSuccess.success);
        
        //Go to Home page
        
        res = await client.GetAsync("/");
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

    }
    
    
    [Fact]
    public async Task ShouldLoginWithRecoveryCode()
    {
        var client = _factory.CreateClient();

        var body = new
        {
            UserNameOrEmail = IdentityDataSeeder.UserEmail_MFA,
            Password = IdentityDataSeeder.UserPassword_MFA
        };

        //Login
        var res = await client.PostAsync("api/account/login",
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();

        var result = JsonSerializerExtensions.DeserializeAnonymousType(content, 
            new { success = true, 
                result = new
                {
                    requiresTwoFactor =  false
                } 
            })!;
        
        Assert.False(result.success);
        Assert.True(result.result.requiresTwoFactor);

        //Go to MFALogin page
        
        res = await client.GetAsync("/account/loginwithrecoverycode?returnUrl=~/");
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        //Login with Code

        var user = await _userManager.FindByEmailAsync(IdentityDataSeeder.UserEmail_MFA);
        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 1);
        
        var loginBody = new
        {
            code = codes.First()
        };
        
        res = await client.PostAsync("api/account/loginRecoveryCode",
            new StringContent(JsonSerializer.Serialize(loginBody), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        content = await res.Content.ReadAsStringAsync();
        
        var confirmSuccess = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = false })!;
        
        Assert.True(confirmSuccess.success);
        
        //Go to Home page
        
        res = await client.GetAsync("/");
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

    }
    
    [Fact]
    // ReSharper disable once InconsistentNaming
    public async Task ShouldLoginWithSMSCode()
    {
        var client = _factory.CreateClient();

        var body = new
        {
            UserNameOrEmail = IdentityDataSeeder.UserEmail_MFA,
            Password = IdentityDataSeeder.UserPassword_MFA
        };

        //Login
        var res = await client.PostAsync("api/account/login",
            new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();

        var result = JsonSerializerExtensions.DeserializeAnonymousType(content, 
            new { success = true, 
                result = new
                {
                    requiresTwoFactor =  false
                } 
            })!;
        
        Assert.False(result.success);
        Assert.True(result.result.requiresTwoFactor);

        //Go to SMS Login code request page
        
        res = await client.GetAsync("/account/SendSMSCode?returnUrl=~/");
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        
        //Send SMS Code
        
        res = await client.PostAsync("api/account/sendSMSCode", null);
        content = await res.Content.ReadAsStringAsync();
        
        var coreResult = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = false, code = "" })!;
        
        //Go to SMS Login page
        
        res = await client.GetAsync("/account/LoginWithSMS?returnUrl=~/");
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        //Login with Code
        
        var loginBody = new
        {
            coreResult.code
        };
        
        res = await client.PostAsync("api/account/loginSMS",
            new StringContent(JsonSerializer.Serialize(loginBody), System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        
        content = await res.Content.ReadAsStringAsync();
        
        var confirmSuccess = JsonSerializerExtensions.DeserializeAnonymousType(content, new { success = false })!;
        
        Assert.True(confirmSuccess.success);
        
        //Go to Home page
        
        res = await client.GetAsync("/");
        
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

    }
}