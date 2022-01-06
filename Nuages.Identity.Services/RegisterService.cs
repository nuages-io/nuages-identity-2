using Nuages.Identity.Services.Models;

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