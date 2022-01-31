using AspNetCore.Identity.Mongo.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Nuages.Identity.Services.Login;

namespace Nuages.Identity.Services.AspNetIdentity;

[BsonIgnoreExtraElements]
public class NuagesApplicationUser : MongoUser<string>
{
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    public DateTime? LastLogin { get; set; }
    public int LoginCount { get; set; }

    public bool UserMustChangePassword { get; set; }
    
    public DateTime CreatedOn { get; set; }
    
    public DateTime? LastPasswordChangedDate { get; set; }
    public bool EnableAutoExpirePassword { get; set; } = true;

    [BsonRepresentation(BsonType.String)]
    public FailedLoginReason? LastFailedLoginReason { get; set; }
    
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    
    public bool LockoutMessageSent { get; set; }
}