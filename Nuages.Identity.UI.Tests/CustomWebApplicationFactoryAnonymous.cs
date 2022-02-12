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

namespace Nuages.Identity.UI.Tests;

// ReSharper disable once ClassNeverInstantiated.Global
public class CustomWebApplicationFactoryAnonymous<TStartup>
    : WebApplicationFactory<TStartup> where TStartup: class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            
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
                options.UseInMemoryDatabase("IdentityContext"));

            var identityBuilder = new IdentityBuilder(typeof(NuagesApplicationUser<string>), typeof(NuagesApplicationRole<string>), services);
            identityBuilder.AddEntityFrameworkStores<TestDataContext>();
            
            
            // var userStoreType = typeof(UserStore<,,,>).MakeGenericType(typeof(NuagesApplicationUser), typeof(NuagesApplicationRole), typeof(IdentityDbContext), typeof(string));
            // var roleStoreType = typeof(RoleStore<,,>).MakeGenericType( typeof(NuagesApplicationRole), typeof(IdentityDbContext), typeof(string));
            // services.AddScoped(typeof(IUserStore<>).MakeGenericType(typeof(NuagesApplicationUser)), userStoreType);
            // services.AddScoped(typeof(IRoleStore<>).MakeGenericType( typeof(NuagesApplicationRole)), roleStoreType);

            
            services.AddHostedService<IdentityDataSeeder>();

            services.AddScoped<IRecaptchaValidator, DummyRecaptchaValidator>();
            services.AddScoped<IMessageSender, DummyMessageSender>();
        });
    }
}