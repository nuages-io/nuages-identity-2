namespace Nuages.Identity.Services.Models;

public class RegisterResultModel
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public string? ConfirmationUrl { get; set; }
}