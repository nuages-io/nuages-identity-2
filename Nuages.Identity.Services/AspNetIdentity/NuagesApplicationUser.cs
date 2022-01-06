using AspNetCore.Identity.Mongo.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nuages.Identity.Services.AspNetIdentity;

public class NuagesApplicationUser : MongoUser<string>
{
    public string? Language { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? UserAgreementVersion { get; set; }
    public string? Country { get; set; }
    
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    public DateTime? LastLogin { get; set; }
    public int LoginCount { get; set; }

    public bool UserMustChangePassword { get; set; }

    public string? PasswordHistory { get; set; }
    
    public DateTime CreatedOn { get; set; }
    
    public DateTime? LastPasswordChangedDate { get; set; }
    public bool EnableAutoExpirePassword { get; set; } = true;
    
    public DateTime? EmailDateTime { get; set; }
    
    public DateTime? PhoneDateTime { get; set; }

    [BsonRepresentation(BsonType.String)]
    public FailedLoginReason? LastFailedLoginReason { get; set; }
}