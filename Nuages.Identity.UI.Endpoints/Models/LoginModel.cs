using System.ComponentModel.DataAnnotations;

namespace Nuages.Identity.UI.Endpoints.Models;

public class LoginModel
{
    [Required] public string UserNameOrEmail { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }

    public string? RecaptchaToken { get; set; } = null!;
}