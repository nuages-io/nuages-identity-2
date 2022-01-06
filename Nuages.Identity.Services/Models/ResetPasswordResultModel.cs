namespace Nuages.Identity.Services.Models;

public class ResetPasswordResultModel
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}