using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Models;

namespace Nuages.Identity.Services;

public class ForgotPasswordService : IForgotPasswordService
{
    private readonly NuagesUserManager _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ForgotPasswordService(NuagesUserManager userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
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

        // For more information on how to enable account confirmation and password reset please
        // visit https://go.microsoft.com/fwlink/?LinkID=532713
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var url =
            $"{_httpContextAccessor.HttpContext!.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Host}/Account/ResetPassword?code{code}";
        

        // await _emailSender.SendEmailAsync(
        //     Input.Email,
        //     "Reset Password",
        //     $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

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