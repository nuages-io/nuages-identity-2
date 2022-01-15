// ReSharper disable InconsistentNaming

using Nuages.Web.Recaptcha;

namespace Nuages.Identity.UI;

public class UIOptions
{
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportUrl { get; set; } = string.Empty;
    public bool ShowRegistration { get; set; } 
    public string RegistrationUrl { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public bool ExternalLoginAutoEnrollIfEmailExists { get; set; }
    public bool ExternalLoginPersistent { get; set; }
}

public static class UIConfigExtension
{
    public static void AddUI(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<UIOptions>(configuration.GetSection("Nuages:UI"));
        
        services.AddGoogleRecaptcha(configuration);
    }
}