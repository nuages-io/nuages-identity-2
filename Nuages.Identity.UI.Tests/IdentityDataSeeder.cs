using Nuages.AspNetIdentity.Core;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace Nuages.Identity.UI.Tests;


public class IdentityDataSeeder : IHostedService
{
    private readonly IServiceProvider _provider;

    public IdentityDataSeeder(IServiceProvider provider)
    {
        _provider = provider;
    }
    
    public const string UserEmail = UserId + "@example.com";
    public const string UserUserName = "user";
    public const string UserPassword = "Nuages123*";
    public const string UserId = "8e8afe93-74e1-47a7-9c02-c907cd37b9b9";
    
    public const string UserEmail_Unconfirmed = UserId_Unconfirmed + "@example.com";
    public const string UserUserName_Unconfirmed = "user_unconfirmed";
    public const string UserPassword_Unconfirmed = "Nuages123*";
    public const string UserId_Unconfirmed = "ee909990-9d75-45e1-b8d3-0e2575671ee2";
    
    public const string UserEmail_MFA = UserId_MFA + "@example.com";
    public const string UserUserName_MFA = "user_mfa";
    public const string UserPassword_MFA = "Nuages123*";
    public const string UserId_MFA = "86e7f6f8-04c4-4043-a884-8bb901e16227";
    
    public const string AdminEmail = AdminId + "@example.com";
    public const string AdminUserName = "admin";
    public const string AdminPassword = "Nuages123*";
    public const string AdminId = "9da1b495-844e-453c-9748-cea28c959819";
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _provider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<NuagesUserManager>();
    
        if (await userManager.FindByEmailAsync(AdminEmail) == null)
        {
            var newUser = new NuagesApplicationUser<string>
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
            var newUser = new NuagesApplicationUser<string>
            {
                Id = UserId,
                UserName = UserUserName,
                Email = UserEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(newUser, UserPassword);
            
        }
        
        if (await userManager.FindByEmailAsync(UserEmail_Unconfirmed) == null)
        {
            var newUser = new NuagesApplicationUser<string>
            {
                Id = UserId_Unconfirmed,
                UserName = UserUserName_Unconfirmed,
                Email = UserEmail_Unconfirmed,
                EmailConfirmed = false
            };
            await userManager.CreateAsync(newUser, UserPassword_Unconfirmed);
            
        }
        
        if (await userManager.FindByEmailAsync(UserEmail_MFA) == null)
        {
            var newUser = new NuagesApplicationUser<string>
            {
                Id = UserId_MFA,
                UserName = UserUserName_MFA,
                Email = UserEmail_MFA,
                EmailConfirmed = true,
                PhoneNumber = "999999999",
                PhoneNumberConfirmed = true
            };
            
            await userManager.CreateAsync(newUser, UserPassword_MFA);
            
            await userManager.ResetAuthenticatorKeyAsync(newUser);
            await userManager.SetTwoFactorEnabledAsync(newUser, true);
            await userManager.GenerateNewTwoFactorRecoveryCodesAsync(newUser, 1);
            
        }
        
        var roleManager = scope.ServiceProvider.GetRequiredService<NuagesRoleManager>();
       
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var adminRole = new NuagesApplicationRole<string>
            {
                Name = "Admin",
                Id = Guid.NewGuid().ToString()
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