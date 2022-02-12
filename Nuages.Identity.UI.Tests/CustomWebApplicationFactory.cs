using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nuages.AspNetIdentity.Core;
using Nuages.AspNetIdentity.Stores.InMemory;
using Nuages.AspNetIdentity.Stores.Mongo;
using Nuages.Sender.API.Sdk;
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
        builder.ConfigureAppConfiguration(builder =>
        {
            builder.AddInMemoryCollection(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Nuages:OpenIdDict:InMemory", "True")
            });
        });
        
        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                    options.DefaultScheme = "Test";
                })
                .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(
                    "Test", options => { options.DefaultUserId = DefaultUserId; });

            // services
            //     .AddSingleton<IInMemoryStorage<NuagesApplicationRole<string>>,
            //         InMemoryStorage<NuagesApplicationRole<string>, string>>();

            // services.AddSingleton(typeof(IUserStore<>).MakeGenericType(typeof(NuagesApplicationUser<string>)),
            //     typeof(InMemoryUserStore<NuagesApplicationUser<string>, NuagesApplicationRole<string>, string>));
            // services.AddSingleton(typeof(IRoleStore<>).MakeGenericType(typeof(NuagesApplicationRole<string>)),
            //     typeof(InMemoryRoleStore<NuagesApplicationRole<string>, string>));

            var serviceDescriptorUser = services.First(s =>  s.ImplementationType != null && s.ImplementationType.Name.Contains("MongoUserStore"));
            services.Remove(serviceDescriptorUser);
            
            var serviceDescriptorRole = services.First(s =>  s.ImplementationType != null && s.ImplementationType.Name.Contains("MongoRoleStore"));
            services.Remove(serviceDescriptorRole);

            services.AddDbContext<TestDataContext>(options =>
            {
                options.UseInMemoryDatabase("IdentityContext");
                options.UseOpenIddict();
            });
               

            var identityBuilder = new IdentityBuilder(typeof(NuagesApplicationUser<string>), typeof(NuagesApplicationRole<string>), services);
            identityBuilder.AddEntityFrameworkStores<TestDataContext>();

            services.AddHostedService<IdentityDataSeeder>();

            services.AddScoped<IRecaptchaValidator, DummyRecaptchaValidator>();
            services.AddScoped<IMessageSender, DummyMessageSender>();
        });
    }
}