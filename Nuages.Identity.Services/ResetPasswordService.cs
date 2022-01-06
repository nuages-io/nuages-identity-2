using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Models;

namespace Nuages.Identity.Services;

public class ResetPasswordService : IResetPasswordService
{
    private readonly NuagesUserManager _userManager;

    public ResetPasswordService(NuagesUserManager userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<ResetPasswordResultModel> Reset(ResetPasswordModel model)
    {
        if (model.Password != model.PasswordConfirm)
        {
            return new ResetPasswordResultModel
            {
                Success = false,
                Errors = new List<string> { "PasswordConfirmDoesNotMatch" }
            };
        }
        
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return new ResetPasswordResultModel
            {
                Success = true //Fake Success
            };
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
        if (result.Succeeded)
        {
            return new ResetPasswordResultModel
            {
                Success = true
            };
        }
        
        var res = new ResetPasswordResultModel
        {
            Success = false
        };

        foreach (var error in result.Errors)
        {
            res.Errors.Add(error.Description);
        }

        return res;
    }
}

public interface IResetPasswordService
{
    Task<ResetPasswordResultModel> Reset(ResetPasswordModel model);
}