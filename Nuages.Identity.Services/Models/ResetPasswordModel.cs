namespace Nuages.Identity.Services.Models;

public class ResetPasswordModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PasswordConfirm { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}