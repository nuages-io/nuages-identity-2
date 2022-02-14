using Microsoft.AspNetCore.Identity;

namespace Nuages.Identity.Services.AspNetIdentity;

// ReSharper disable once ClassNeverInstantiated.Global
public class NuagesApplicationRole<TKey> : IdentityRole<TKey> where TKey : IEquatable<TKey>
{
}