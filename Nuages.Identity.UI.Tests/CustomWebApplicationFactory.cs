using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nuages.AspNetIdentity.Core;
using Nuages.AspNetIdentity.Stores.InMemory;
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

            services
                .AddSingleton<IInMemoryStorage<NuagesApplicationRole>,
                    InMemoryStorage<NuagesApplicationRole, string>>();

            services.AddSingleton(typeof(IUserStore<>).MakeGenericType(typeof(NuagesApplicationUser)),
                typeof(InMemoryUserStore<NuagesApplicationUser, NuagesApplicationRole, string>));
            services.AddSingleton(typeof(IRoleStore<>).MakeGenericType(typeof(NuagesApplicationRole)),
                typeof(InMemoryRoleStore<NuagesApplicationRole, string>));

            // services.AddDbContext<IdentityDbContext<NuagesApplicationUser, NuagesApplicationRole, string>>(options =>
            //     options.UseInMemoryDatabase("IdentityContext"));

            // var identityBuilder = new IdentityBuilder(typeof(NuagesApplicationUser), typeof(NuagesApplicationRole), services);
            // identityBuilder.AddEntityFrameworkStores<IdentityDbContext>();
            
            // var userStoreType = typeof(UserStore<,,,>).MakeGenericType(typeof(NuagesApplicationUser), typeof(NuagesApplicationRole), typeof(IdentityDbContext), typeof(string));
            // var roleStoreType = typeof(RoleStore<,,>).MakeGenericType( typeof(NuagesApplicationRole), typeof(IdentityDbContext), typeof(string));
            // services.AddScoped(typeof(IUserStore<>).MakeGenericType(typeof(NuagesApplicationUser)), userStoreType);
            // services.AddScoped(typeof(IRoleStore<>).MakeGenericType( typeof(NuagesApplicationRole)), roleStoreType);
            //
            services.AddHostedService<IdentityDataSeeder>();

            services.AddScoped<IRecaptchaValidator, DummyRecaptchaValidator>();
            services.AddScoped<IMessageSender, DummyMessageSender>();
        });
    }
}