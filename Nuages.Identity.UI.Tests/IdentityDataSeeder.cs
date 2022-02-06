using Nuages.AspNetIdentity.Core;
// ReSharper disable MemberCanBePrivate.Global

namespace Nuages.Identity.UI.Tests;


public class IdentityDataSeeder : IHostedService
{
    private readonly IServiceProvider _provider;

    public IdentityDataSeeder(IServiceProvider provider)
    {
        _provider = provider;
    }
    
    public const string UserEmail = "8e8afe93-74e1-47a7-9c02-c907cd37b9b9@example.com";
    public const string UserUserName = "user";
    public const string UserPassword = "Nuages123*";
    public const string UserId = "8e8afe93-74e1-47a7-9c02-c907cd37b9b9";
    
    public const string AdminEmail = "9da1b495-844e-453c-9748-cea28c959819@example.com";
    public const string AdminUserName = "admin";
    public const string AdminPassword = "Nuages123*";
    public const string AdminId = "9da1b495-844e-453c-9748-cea28c959819";
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<NuagesUserManager>();
    
        if (await userManager.FindByEmailAsync(AdminEmail) == null)
        {
            var newUser = new NuagesApplicationUser
            {
                Id = AdminId,
                UserName = AdminUserName,
                Email = AdminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(newUser, AdminPassword);
        }
        
        if (await userManager.FindByEmailAsync(UserEmail) == null)
        {
            var newUser = new NuagesApplicationUser
            {
                Id = UserId,
                UserName = UserUserName,
                Email = UserEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(newUser, UserPassword);
            
            await userManager.SetTwoFactorEnabledAsync(newUser, true);
        }
        
        var roleManager = scope.ServiceProvider.GetRequiredService<NuagesRoleManager>();
       
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var adminRole = new NuagesApplicationRole
            {
                Name = "Admin"
            };
            
            await roleManager.CreateAsync(adminRole);
        }

        var existingUser = await userManager.FindByEmailAsync(AdminEmail);

        if (!await userManager.IsInRoleAsync(existingUser, "Admin"))
        {
            await userManager.AddToRoleAsync(existingUser, "Admin");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}