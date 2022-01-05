using Microsoft.AspNetCore.Identity;
using Nuages.Identity.Services;

namespace Nuages.Identity.UI.Endpoints.Models;

public class LoginResultModel
{
    public bool Success { get; set; }
    public SignInResult Result { get; set; } = null!;
    public string? Message { get; set; }
    public SignInFailedReason? Reason { get; set; }
}