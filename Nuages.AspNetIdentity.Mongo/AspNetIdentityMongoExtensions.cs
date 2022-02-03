using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Nuages.AspNetIdentity;

namespace Nuages.Identity.Services.AspNetIdentity.Mongo;

public static class AspNetIdentityMongoExtensions
{
    public static void AddMongoStorage(this IdentityBuilder builder, Action<MongoIdentityOptions> configure)
    {
        builder.Services.Configure(configure);
        
        MapModel();

        builder.AddUserStore<MongoUserStore<NuagesApplicationUser, NuagesApplicationRole, string>>();
        builder.AddRoleStore<MongoRoleStore<NuagesApplicationRole, string>>();
    }

    private static void MapModel()
    {
     
        BsonClassMap.RegisterClassMap<MongoIdentityUserClaim<string>>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Id);
            cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });
        
        BsonClassMap.RegisterClassMap<MongoIdentityUserLogin<string>>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Id);
            cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<IdentityUserLogin<string>>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });
        
        BsonClassMap.RegisterClassMap<MongoIdentityUserRole<string>>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Id);
            cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<IdentityUserRole<string>>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });


        BsonClassMap.RegisterClassMap<IdentityRole<string>>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(c => c.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<MongoIdentityUserToken<string>>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Id);
            cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<IdentityUserToken<string>>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<IdentityUser<string>>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(c => c.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<NuagesApplicationUser>(cm =>
        {
            cm.AutoMap();
            cm.SetIgnoreExtraElements(true);
            cm.MapMember(c => c.LastFailedLoginReason)
                .SetSerializer(new EnumSerializer<FailedLoginReason>(BsonType.String));
        });
    }
}