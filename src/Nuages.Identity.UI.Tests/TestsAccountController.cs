using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Login;
using Nuages.Identity.Services.Login.MagicLink;
using Nuages.Identity.Services.Password;
using Nuages.Identity.Services.Register;
using OtpNet;
using Xunit;

namespace Nuages.Identity.UI.Tests;

[Collection("IntegrationTestUI")]
public class TestsAccountController : IClassFixture<CustomWebApplicationFactoryAnonymous<Program>>
{
    private readonly CustomWebApplicationFactoryAnonymous<Program> _factory;
    private readonly JsonSerializerOptions _options;
    private readonly NuagesUserManager _userManager;

    public TestsAccountController(CustomWebApplicationFactoryAnonymous<Program> factory)
    {
        _factory = factory;

        var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        var scope = scopeFactory.CreateScope();

        _userManager = scope.ServiceProvider.GetRequiredService<NuagesUserManager>();

        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        _options.Converters.Add(new JsonStringEnumConverter());
    }

    [Fact]
    public async Task ShouldLoginWithSuccess()
    {
        var client = _factory.CreateClient();

        var body = new LoginModel
        {
            UserNameOrEmail = IdentityDataSeeder.UserEmail,
            Password = IdentityDataSeeder.UserPassword
        };

        var res = await client.PostAsync("api/account/login",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();

        var response = JsonSerializer.Deserialize<LoginResultModel>(content, _options);

        Assert.True(response!.Success);
    }

    [Fact]
    public async Task ShouldLForgotPasswordWithSuccess()
    {
        var client = _factory.CreateClient();

        var body = new ForgotPasswordModel
        {
            Email = IdentityDataSeeder.UserEmail
        };

        var res = await client.PostAsync("api/account/forgotPassword",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<ForgotPasswordResultModel>(content, _options);

        Assert.True(result!.Success);

        //Go to reset page

        res = await client.GetAsync(result.Url);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var resetBody = new ResetPasswordModel
        {
            Email = IdentityDataSeeder.UserEmail,
            Password = "Nuages123*",
            PasswordConfirm = "Nuages123*",
            Code = result.Code!
        };

        res = await client.PostAsync("api/account/resetPassword",
            new StringContent(JsonSerializer.Serialize(resetBody), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        content = await res.Content.ReadAsStringAsync();

        var result2 = JsonSerializer.Deserialize<ResetPasswordResultModel>(content, _options);

        Assert.True(result2!.Success);
    }

    [Fact]
    public async Task ShouldStartMagicLinkWithSuccess()
    {
        var client = _factory.CreateClient();

        var body = new StartMagicLinkModel
        {
            Email = IdentityDataSeeder.UserEmail
        };

        var res = await client.PostAsync("api/account/magicLinkLogin",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<StartMagicLinkResultModel>(content, _options);

        Assert.True(result!.Success);
        Assert.NotNull(result.Url);

        res = await client.GetAsync(result.Url);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task ShouldLoginWithErrorUnconfirmed()
    {
        var client = _factory.CreateClient();

        var body = new LoginModel
        {
            UserNameOrEmail = IdentityDataSeeder.UserEmail_Unconfirmed,
            Password = IdentityDataSeeder.UserPassword_Unconfirmed
        };

        //Login
        var res = await client.PostAsync("api/account/login",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();


        var result = JsonSerializer.Deserialize<LoginResultModel>(content, _options);

        Assert.False(result!.Success);
        Assert.Equal(FailedLoginReason.EmailNotConfirmed, result.Reason);

        //Got to EmailNotCOnfirmed page
        res = await client.GetAsync("/account/emailnotconfirmed");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        //Send confirmation link

        res = await client.PostAsync("api/account/sendEmailConfirmation",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        content = await res.Content.ReadAsStringAsync();

        var confirmResult = JsonSerializer.Deserialize<SendEmailConfirmationResultModel>(content, _options);

        //Confirm email
        res = await client.GetAsync(confirmResult!.Url);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        //Login again

        res = await client.PostAsync("api/account/login",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        content = await res.Content.ReadAsStringAsync();

        var loginResult = JsonSerializer.Deserialize<LoginResultModel>(content, _options);

        Assert.True(loginResult!.Success);
    }

    [Fact]
    public async Task ShouldLoginWithMfa()
    {
        var client = _factory.CreateClient();

        var body = new LoginModel
        {
            UserNameOrEmail = IdentityDataSeeder.UserEmail_MFA,
            Password = IdentityDataSeeder.UserPassword_MFA
        };

        //Login
        var res = await client.PostAsync("api/account/login",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<LoginResultModel>(content, _options);


        Assert.False(result!.Success);
        Assert.True(result.Result.RequiresTwoFactor);

        //Go to MFALogin page

        res = await client.GetAsync("/account/loginwith2fa?returnUrl=~/");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        //Login with Code

        var user = await _userManager.FindByEmailAsync(IdentityDataSeeder.UserEmail_MFA);
        var key = await _userManager.GetAuthenticatorKeyAsync(user);

        var totp = new Totp(Base32Encoding.ToBytes(key)).ComputeTotp();

        var loginBody = new Login2FAModel
        {
            Code = totp
        };

        res = await client.PostAsync("api/account/login2fa",
            new StringContent(JsonSerializer.Serialize(loginBody), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        content = await res.Content.ReadAsStringAsync();

        var confirmSuccess = JsonSerializer.Deserialize<LoginResultModel>(content, _options);

        Assert.True(confirmSuccess!.Success);

        //Go to Home page

        res = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }


    [Fact]
    public async Task ShouldLoginWithRecoveryCode()
    {
        var client = _factory.CreateClient();

        var body = new LoginModel
        {
            UserNameOrEmail = IdentityDataSeeder.UserEmail_MFA,
            Password = IdentityDataSeeder.UserPassword_MFA
        };

        //Login
        var res = await client.PostAsync("api/account/login",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<LoginResultModel>(content, _options);

        Assert.False(result!.Success);
        Assert.True(result.Result.RequiresTwoFactor);

        //Go to MFALogin page

        res = await client.GetAsync("/account/loginwithrecoverycode?returnUrl=~/");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        //Login with Code

        var user = await _userManager.FindByEmailAsync(IdentityDataSeeder.UserEmail_MFA);
        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 1);

        var loginBody = new LoginRecoveryCodeModel
        {
            Code = codes.First()
        };

        res = await client.PostAsync("api/account/loginRecoveryCode",
            new StringContent(JsonSerializer.Serialize(loginBody), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        content = await res.Content.ReadAsStringAsync();

        var confirmSuccess = JsonSerializer.Deserialize<LoginResultModel>(content, _options);

        Assert.True(confirmSuccess!.Success);

        //Go to Home page

        res = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    // ReSharper disable once InconsistentNaming
    public async Task ShouldLoginWithSMSCode()
    {
        var client = _factory.CreateClient();

        var body = new LoginModel
        {
            UserNameOrEmail = IdentityDataSeeder.UserEmail_MFA,
            Password = IdentityDataSeeder.UserPassword_MFA
        };

        //Login
        var res = await client.PostAsync("api/account/login",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<LoginResultModel>(content, _options);

        Assert.False(result!.Success);
        Assert.True(result.Result.RequiresTwoFactor);

        //Go to SMS Login code request page

        res = await client.GetAsync("/account/SendSMSCode?returnUrl=~/");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);


        //Send SMS Code

        res = await client.PostAsync("api/account/sendSMSCode", null);
        content = await res.Content.ReadAsStringAsync();

        var codeResult = JsonSerializer.Deserialize<SendSMSCodeResultModel>(content, _options);
        //Go to SMS Login page

        res = await client.GetAsync("/account/LoginWithSMS?returnUrl=~/");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        //Login with Code

        var loginBody = new LoginSMSModel
        {
            Code = codeResult!.Code!
        };

        res = await client.PostAsync("api/account/loginSMS",
            new StringContent(JsonSerializer.Serialize(loginBody), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        content = await res.Content.ReadAsStringAsync();

        var confirmSuccess = JsonSerializer.Deserialize<LoginResultModel>(content, _options);

        Assert.True(confirmSuccess!.Success);

        //Go to Home page

        res = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task ShouldRegisterWithSuccess()
    {
        var client = _factory.CreateClient();

        var body = new RegisterModel
        {
            Email = "newUser@example.com",
            Password = IdentityDataSeeder.UserPassword,
            PasswordConfirm = IdentityDataSeeder.UserPassword
        };

        var res = await client.PostAsync("api/account/register",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var content = await res.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<RegisterResultModel>(content, _options);

        Assert.True(result!.Success);
    }
}