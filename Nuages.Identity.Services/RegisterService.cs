

namespace Nuages.Identity.Services;

public class RegisterService : IRegisterService
{
    public Task<RegisterResultModel> Register(RegisterModel model)
    {
        throw new NotImplementedException();
    }
}

public interface IRegisterService
{
    Task<RegisterResultModel> Register(RegisterModel model);
}

public class RegisterModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
public class RegisterResultModel
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public string? ConfirmationUrl { get; set; }
}