using Microsoft.AspNetCore.Identity;

namespace Nuages.AspNetIdentity.Core;

// ReSharper disable once ClassNeverInstantiated.Global
public class NuagesApplicationRole<TKey> : IdentityRole<TKey> where TKey : IEquatable<TKey>
{
}