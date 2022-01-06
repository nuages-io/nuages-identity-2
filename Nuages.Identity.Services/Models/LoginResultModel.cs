using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.Models;

public class LoginResultModel
{
    public bool Success { get; set; }
    public SignInResult Result { get; set; } = null!;
    public string? Message { get; set; }
    public FailedLoginReason? Reason { get; set; }
}