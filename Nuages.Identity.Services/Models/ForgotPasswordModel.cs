namespace Nuages.Identity.Services.Models;

public class ForgotPasswordModel
{
    public string Email { get; set; } = string.Empty;
    public string? RecaptchaToken { get; set; }
}