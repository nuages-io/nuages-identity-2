using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Web;
using Nuages.Web.Recaptcha;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Nuages.Identity.UI.Tests;

// ReSharper disable once ClassNeverInstantiated.Global
public class CustomWebApplicationFactory<TStartup>
    : WebApplicationFactory<TStartup> where TStartup : class
{
    public string DefaultUserId { get; set; } = IdentityDataSeeder.UserId;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Program.ConfigurationOverrides = new List<KeyValuePair<string, string?>> 
            { new("Nuages:Data:Storage", "InMemory"), 
            new("Nuages:OpenIdDict:Storage", "InMemory"),
            new("Nuages:UseCookiePolicy", "false")
    };

    // builder.ConfigureAppConfiguration(configurationBuilder =>
        // {
        //     configurationBuilder.AddInMemoryCollection(new List<KeyValuePair<string, string>>
        //     {
        //         new("Nuages:Data:Storage", "InMemory"),
        //         new("Nuages:OpenIdDict:Storage", "InMemory"),
        //         new("Nuages:UseCookiePolicy", "false")
        //         
        //     });
        // });

        builder.ConfigureTestServices(services =>
        {
            services
                .AddMvc()
                .AddMvcOptions(options => { options.Filters.Clear(); });
                
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                    options.DefaultScheme = "Test";
                })
                .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(
                    "Test", options => { options.DefaultUserId = DefaultUserId; });
            
            services.Configure<CookiePolicyOptions>(options => { options.Secure = CookieSecurePolicy.None; });
            
            var serviceDescriptorUser = services.FirstOrDefault(s =>
                s.ImplementationType != null && s.ImplementationType.Name.Contains("MongoUserStore"));
            if (serviceDescriptorUser != null)
                services.Remove(serviceDescriptorUser);

            var serviceDescriptorRole = services.FirstOrDefault(s =>
                s.ImplementationType != null && s.ImplementationType.Name.Contains("MongoRoleStore"));
            if (serviceDescriptorRole != null)
                services.Remove(serviceDescriptorRole);

            services.AddDbContext<TestDataContext>(options =>
            {
                options.UseInMemoryDatabase("IdentityContext");
                options.UseOpenIddict();
            });

            var identityBuilder = new IdentityBuilder(typeof(NuagesApplicationUser<string>),
                typeof(NuagesApplicationRole<string>), services);
            identityBuilder.AddEntityFrameworkStores<TestDataContext>();

            services.AddHostedService<IdentityDataSeeder>();

            services.AddScoped<IRecaptchaValidator, DummyRecaptchaValidator>();
            services.AddScoped<IEmailMessageSender, DummyEmailMessageSender>();
            
            services.AddScoped<IRuntimeConfiguration, RuntimeTestsConfiguration>();
        });
    }
}