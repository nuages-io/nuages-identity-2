using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Nuages.Fido2.Storage.Mongo;

public class MongoSchemaInitializer : IHostedService
{
    public MongoSchemaInitializer(IOptions<Fido2MongoOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        
        var mongoUrl = new MongoUrl(options.Value.ConnectionString);
        var database = client.GetDatabase(mongoUrl.DatabaseName);

        Fido2CredentialCollection = database.GetCollection<Fido2Credential>("fido2_credentials");
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Fido2CredentialCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<Fido2Credential>(
                Builders<Fido2Credential>.IndexKeys
                    .Ascending(p => p.UserHandle)
                , new CreateIndexOptions
                {
                    Name = "IX_UserHandle",
                    Unique = false
                }), cancellationToken: cancellationToken
        );
        
        await Fido2CredentialCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<Fido2Credential>(
                Builders<Fido2Credential>.IndexKeys
                    .Ascending(p => p.UserId)
                , new CreateIndexOptions
                {
                    Name = "IX_UserId",
                    Unique = false
                }), cancellationToken: cancellationToken
        );
        
        await Fido2CredentialCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<Fido2Credential>(
                Builders<Fido2Credential>.IndexKeys
                    .Ascending(p => p.Descriptor.Id)
                , new CreateIndexOptions
                {
                    Name = "UX_DescriptionId",
                    Unique = true
                }), cancellationToken: cancellationToken
        );
        
        await Fido2CredentialCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<Fido2Credential>(
                Builders<Fido2Credential>.IndexKeys
                    .Ascending(p => p.UserId)
                    .Ascending(p => p.Descriptor.Id)
                , new CreateIndexOptions
                {
                    Name = "IX_UserIdDescriptionId",
                    Unique = false
                }), cancellationToken: cancellationToken
        );
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public IMongoCollection<Fido2Credential> Fido2CredentialCollection { get; set; }
}