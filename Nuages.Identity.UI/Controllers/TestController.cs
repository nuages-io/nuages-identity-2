using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.Services.AspNetIdentity;
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Controllers;

[Route("[controller]")]
public class TestController
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesRoleManager _roleManager;

    public TestController(NuagesUserManager userManager, NuagesRoleManager roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    
#if DEBUG
    [HttpGet("generate")]
    public async Task GenerateDummyData(int count = 10)
    {
        var roleCLaims = new[] { 0, 1, 2 };
        
        var roles = new List<NuagesApplicationRole<string>>();
        
        for (var i = 0; i < 10; i++)
        {
            var role = new NuagesApplicationRole<string>
            {
                Name = $"Role{1}"
            };

            await _roleManager.CreateAsync(role);

            roles.Add(role);
            
            foreach (var c in roleCLaims)
            {
                await _roleManager.AddClaimAsync(role, new Claim($"Claim{c}", $"Value{c}"));
            }
            
        }
        
        var userClaims = new[] { 0, 1, 2, 3, 4, 5 };
        
        for (var i = 0; i < count; i++)
        {
            var user = new NuagesApplicationUser<string>
            {
                Email = $"test{i}@example.com",
                UserName = $"test{i}@example.com",
                EmailConfirmed = true
            };

            var res = await _userManager.CreateAsync(user);
            if (res.Succeeded)
            {
                await _userManager.AddPasswordAsync(user, "TestPass123*");
            }

            await _userManager.AddToRolesAsync(user, roles.Select(r => r.Name));

            await _userManager.AddClaimsAsync(user, userClaims.Select(r => new Claim($"Claim{r}", $"Value{r}")));
        }
    }


    // [HttpGet("Perf")]
    // public async Task<ActionResult> TestPerf()
    // {
    //     var time = DateTime.UtcNow;
    //     
    //     var user = await _userManager.FindByEmailAsync("test500@example.com");
    //     var claims = (await _userManager.GetClaimsAsync(user)).ToList();
    //     var roles = (await _userManager.GetRolesAsync(user)).ToList();
    //     var users = (await _userManager.GetUsersForClaimAsync(new Claim("Claim1", "Value1"))).ToList();
    //     var usersInROle = (await _userManager.GetUsersInRoleAsync("Role1")).ToList();
    //     
    //
    //     return new ContentResult
    //     {
    //         Content = (DateTime.UtcNow - time).TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
    //         ContentType = "application/text",
    //         StatusCode = 200
    //     };
    // }
#endif
}