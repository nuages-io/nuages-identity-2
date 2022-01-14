using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nuages.Identity.API.Endpoints;

[ExcludeFromCodeCoverage]
[Authorize]
[Route("api/admin")]
public class AdminController
{
    public async Task<bool> RemoveAuthenticator()
    {
        return await Task.FromResult(true);
    }
    
    public async Task<bool> ChangeEmail()
    {
        return await Task.FromResult(true);
    }
    
    public async Task<bool> ChangeUserName()
    {
        return await Task.FromResult(true);
    }
    
    public async Task<bool> ChengePhoneNumber()
    {
        return await Task.FromResult(true);
    }
    
    public async Task<bool> ChangePassword()
    {
        return await Task.FromResult(true);
    }
}