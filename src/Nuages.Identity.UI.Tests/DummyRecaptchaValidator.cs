using Nuages.Web.Recaptcha;

namespace Nuages.Identity.UI.Tests;

public class DummyRecaptchaValidator : IRecaptchaValidator
{
    public Task<bool> ValidateAsync(string? token)
    {
        return Task.FromResult(true);
    }
}