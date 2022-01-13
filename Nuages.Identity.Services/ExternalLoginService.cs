namespace Nuages.Identity.Services;

public class ExternalLoginService : IExternalLoginService
{
    //     public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
    // {
    //     returnUrl = returnUrl ?? Url.Content("~/");
    //     // Get the information about the user from the external login provider
    //     var info = await _signInManager.GetExternalLoginInfoAsync();
    //     if (info == null)
    //     {
    //         ErrorMessage = "Error loading external login information during confirmation.";
    //         return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
    //     }
    //
    //     if (ModelState.IsValid)
    //     {
    //         var user = CreateUser();
    //
    //         await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
    //         await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
    //
    //         var result = await _userManager.CreateAsync(user);
    //         if (result.Succeeded)
    //         {
    //             result = await _userManager.AddLoginAsync(user, info);
    //             if (result.Succeeded)
    //             {
    //                 _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
    //
    //                 var userId = await _userManager.GetUserIdAsync(user);
    //                 var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
    //                 code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
    //                 var callbackUrl = Url.Page(
    //                     "/Account/ConfirmEmail",
    //                     pageHandler: null,
    //                     values: new { area = "Identity", userId = userId, code = code },
    //                     protocol: Request.Scheme);
    //
    //                 // await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
    //                 //     $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
    //
    //                 // If account confirmation is required, we need to show the link if we don't have a real email sender
    //                 if (_userManager.Options.SignIn.RequireConfirmedAccount)
    //                 {
    //                     return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
    //                 }
    //
    //                 await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
    //                 return LocalRedirect(returnUrl);
    //             }
    //         }
    //         foreach (var error in result.Errors)
    //         {
    //             ModelState.AddModelError(string.Empty, error.Description);
    //         }
    //     }
    //
    //     ProviderDisplayName = info.ProviderDisplayName;
    //     ReturnUrl = returnUrl;
    //     return Page();
    // }

}

public interface IExternalLoginService
{
    
}