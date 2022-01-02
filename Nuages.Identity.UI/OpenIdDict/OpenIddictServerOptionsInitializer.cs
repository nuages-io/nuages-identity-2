using Microsoft.Extensions.Options;
using OpenIddict.Server;

namespace Nuages.Identity.UI.OpenidDict;

public class OpenIddictServerOptionsInitializer : IConfigureNamedOptions<OpenIddictServerOptions>
{
    // private readonly ITenantContext _tenantContext;
    // private readonly IServiceProvider _serviceProvider;
    //
    // public OpenIddictServerOptionsInitializer(
    //     ITenantContext tenantContext, IServiceProvider serviceProvider)
    // {
    //     _tenantContext = tenantContext;
    //     _serviceProvider = serviceProvider;
    // }
    //
    // public void Configure(string name, OpenIddictServerOptions options) => Configure(options);
    //
    // public void Configure(OpenIddictServerOptions options)
    // {
    //     var tenant = _tenantContext.GetCurrentAsync().Result;
    //
    //     SetupKeys(options, tenant);
    //
    //     options.Scopes.UnionWith(new []{OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles/*, "https://localhost:5002"*/});
    //        
    //         
    //     // Other tenant-specific options can be registered here.
    // }
    //
    // private void SetupKeys(OpenIddictServerOptions options, Tenant tenant)
    // {
    //     SetupEncryptionKey(options, tenant);
    //
    //     // var signingCredentials = new SigningCredentials(CreateRsaSecurityKey(2048), SecurityAlgorithms.RsaSha256);
    //     // options.SigningCredentials.Add(signingCredentials);
    //
    //     SetupSigningKey(options, tenant);
    // }
    //
    // private void SetupEncryptionKey(OpenIddictServerOptions options, Tenant tenant)
    // {
    //     RsaSecurityKey? key;
    //
    //     if (!string.IsNullOrEmpty(tenant.EncryptionKeyXml))
    //     {
    //         key = CreateRsaSecurityKey(tenant.EncryptionKeyXml);
    //     }
    //     else
    //     {
    //         using var scope  = _serviceProvider.CreateScope();
    //             
    //         key = CreateRsaSecurityKey(2048);
    //         tenant.EncryptionKeyXml =  key.Rsa.ToXmlString(true);
    //             
    //         var tenantDataService = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
    //         Task.Run(async () => await tenantDataService.UpdateAsync(tenant));
    //     }
    //         
    //     var encryptionCredential = new EncryptingCredentials(key,
    //         SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512);
    //
    //     options.EncryptionCredentials.Add(encryptionCredential);
    // }
    //     
    // private void SetupSigningKey(OpenIddictServerOptions options, Tenant tenant)
    // {
    //     RsaSecurityKey? key;
    //
    //     if (!string.IsNullOrEmpty(tenant.SigningKeyXml))
    //     {
    //         key = CreateRsaSecurityKey(tenant.SigningKeyXml);
    //     }
    //     else
    //     {
    //         using var scope  = _serviceProvider.CreateScope();
    //         key = CreateRsaSecurityKey(2048);
    //         tenant.SigningKeyXml = key.Rsa.ToXmlString(true);
    //            
    //             
    //         var tenantDataService = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
    //         Task.Run(async () => await tenantDataService.UpdateAsync(tenant));
    //     }
    //         
    //     var signingCredential = new SigningCredentials(key,
    //         SecurityAlgorithms.RsaSha256);
    //
    //     options.SigningCredentials.Add(signingCredential);
    // }
    //
    // private static RsaSecurityKey CreateRsaSecurityKey(int size)
    // {
    //     var rsa = RSA.Create(size);
    //         
    //     return new RsaSecurityKey(rsa);
    // }
    //
    // private static RsaSecurityKey CreateRsaSecurityKey(string xml)
    // {
    //     var rsa = RSA.Create();
    //     rsa.FromXmlString(xml);
    //         
    //     return new RsaSecurityKey(rsa);
    // }
    public void Configure(OpenIddictServerOptions options)
    {
        throw new NotImplementedException();
    }

    public void Configure(string name, OpenIddictServerOptions options)
    {
        throw new NotImplementedException();
    }
}