using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Nuages.AspNetIdentity.Mongo;

public static class AspNetIdentityMongoExtensions
{
    public static void AddMongoStores<TUser, TRole, TKey>(this IdentityBuilder builder, Action<MongoIdentityOptions> configure)  
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        builder.Services.Configure(configure);
        
        MapModel<TKey>();

        builder.AddUserStore<MongoUserStore<TUser, TRole, TKey>>();
        builder.AddRoleStore<MongoRoleStore<TRole, TKey>>();
    }

    private static void MapModel<TKey>()  
        where TKey : IEquatable<TKey>
    {
     
        BsonClassMap.RegisterClassMap<MongoIdentityUserClaim<TKey>>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Id);
            cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });
        
        BsonClassMap.RegisterClassMap<MongoIdentityUserLogin<TKey>>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Id);
            cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<IdentityUserLogin<TKey>>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });
        
        BsonClassMap.RegisterClassMap<MongoIdentityUserRole<TKey>>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Id);
            cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<IdentityUserRole<TKey>>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });


        BsonClassMap.RegisterClassMap<IdentityRole<TKey>>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(c => c.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<MongoIdentityUserToken<TKey>>(cm =>
        {
            cm.AutoMap();
            cm.MapIdMember(c => c.Id);
            cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<IdentityUserToken<TKey>>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

        BsonClassMap.RegisterClassMap<IdentityUser<TKey>>(cm =>
        {
            cm.AutoMap();
            cm.MapMember(c => c.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
        });

       
    }
}