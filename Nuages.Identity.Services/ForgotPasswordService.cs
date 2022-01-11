using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Sender.API.Sdk;

namespace Nuages.Identity.Services;

public class ForgotPasswordService : IForgotPasswordService
{
    private readonly NuagesUserManager _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordService(NuagesUserManager userManager, IHttpContextAccessor httpContextAccessor, IEmailSender emailSender)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _emailSender = emailSender;
    }
    
    public async Task<ForgotPasswordResultModel> ForgotPassword(ForgotPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        {
            return new ForgotPasswordResultModel
            {
                Success = true // Fake success
            };
        }

       
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var url =
            $"{_httpContextAccessor.HttpContext!.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Host}/Account/ResetPassword?code={code}";

        await _emailSender.SendEmailUsingTemplateAsync(model.Email, "Password_Reset", new Dictionary<string, string>
        {
            { "Link", url }
        });

        return new ForgotPasswordResultModel
        {
            Success = true // Fake success
        };
    }
}

public interface IForgotPasswordService
{
    Task<ForgotPasswordResultModel> ForgotPassword(ForgotPasswordModel model);
}

public class ForgotPasswordModel
{
    public string Email { get; set; } = string.Empty;
    public string? RecaptchaToken { get; set; }
}

public class ForgotPasswordResultModel
{
    public bool Success { get; set; }
}