using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Nuages.AspNetIdentity.Stores.Mongo;

public static class ModelMapper
{
    public static void MapModel<TKey>()  
        where TKey : IEquatable<TKey>
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(MongoIdentityUserClaim<TKey>)))
        {
            BsonClassMap.RegisterClassMap<MongoIdentityUserClaim<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
                cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(MongoIdentityUserLogin<TKey>)))
        {
            BsonClassMap.RegisterClassMap<MongoIdentityUserLogin<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
                cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityUserLogin<TKey>)))
        {
            BsonClassMap.RegisterClassMap<IdentityUserLogin<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(MongoIdentityUserRole<TKey>)))
        {
            BsonClassMap.RegisterClassMap<MongoIdentityUserRole<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
                cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityUserRole<TKey>)))
        {
            BsonClassMap.RegisterClassMap<IdentityUserRole<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityRole<TKey>)))
        {
            BsonClassMap.RegisterClassMap<IdentityRole<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(MongoIdentityUserToken<TKey>)))
        {
            BsonClassMap.RegisterClassMap<MongoIdentityUserToken<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
                cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityUserToken<TKey>)))
        {
            BsonClassMap.RegisterClassMap<IdentityUserToken<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.UserId).SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(IdentityUser<TKey>)))
        {
            BsonClassMap.RegisterClassMap<IdentityUser<TKey>>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }
        

       
    }
}