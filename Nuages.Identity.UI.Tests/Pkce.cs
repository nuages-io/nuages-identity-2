using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable NotAccessedField.Global

namespace Nuages.Identity.UI.Tests;

/// <summary>
/// Provides a randomly generating PKCE code verifier and it's corresponding code challenge.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class Pkce
{
    /// <summary>
    /// The randomly generating PKCE code verifier.
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public string CodeVerifier;

    /// <summary>
    /// Corresponding PKCE code challenge.
    /// </summary>
    public string CodeChallenge;

    /// <summary>
    /// Initializes a new instance of the Pkce class.
    /// </summary>
    /// <param name="size">The size of the code verifier (43 - 128 charters).</param>
    public Pkce(uint size = 128)
    {
        CodeVerifier = GenerateCodeVerifier(size);
        CodeChallenge = GenerateCodeChallenge(CodeVerifier);
    }

    /// <summary>
    /// Generates a code_verifier based on rfc-7636.
    /// </summary>
    /// <param name="size">The size of the code verifier (43 - 128 charters).</param>
    /// <returns>A code verifier.</returns>
    /// <remarks> 
    /// code_verifier = high-entropy cryptographic random STRING using the 
    /// unreserved characters[A - Z] / [a-z] / [0-9] / "-" / "." / "_" / "~"
    /// from Section 2.3 of[RFC3986], with a minimum length of 43 characters
    /// and a maximum length of 128 characters.
    ///    
    /// ABNF for "code_verifier" is as follows.
    ///    
    /// code-verifier = 43*128unreserved
    /// unreserved = ALPHA / DIGIT / "-" / "." / "_" / "~"
    /// ALPHA = %x41-5A / %x61-7A
    /// DIGIT = % x30 - 39 
    ///    
    /// Reference: rfc-7636 https://datatracker.ietf.org/doc/html/rfc7636#section-4.1     
    ///</remarks>
    public static string GenerateCodeVerifier(uint size = 128)
    {
        if (size is < 43 or > 128)
            size = 128;

        const string unreservedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz123456789-._~";
        var random = new Random();
        var highEntropyCryptograph = new char[size];

        for (var i = 0; i < highEntropyCryptograph.Length; i++)
        {
            highEntropyCryptograph[i] = unreservedCharacters[random.Next(unreservedCharacters.Length)];
        }

        return new string(highEntropyCryptograph);
    }

    /// <summary>
    /// Generates a code_challenge based on rfc-7636.
    /// </summary>
    /// <param name="codeVerifier">The code verifier.</param>
    /// <returns>A code challenge.</returns>
    /// <remarks> 
    /// plain
    ///    code_challenge = code_verifier
    ///    
    /// S256
    ///    code_challenge = BASE64URL-ENCODE(SHA256(ASCII(code_verifier)))
    ///    
    /// If the client is capable of using "S256", it MUST use "S256", as
    /// "S256" is Mandatory To Implement(MTI) on the server.Clients are
    /// permitted to use "plain" only if they cannot support "S256" for some
    /// technical reason and know via out-of-band configuration that the
    /// server supports "plain".
    /// 
    /// The plain transformation is for compatibility with existing
    /// deployments and for constrained environments that can't use the S256
    /// transformation.
    ///    
    /// ABNF for "code_challenge" is as follows.
    ///    
    /// code-challenge = 43 * 128unreserved
    /// unreserved = ALPHA / DIGIT / "-" / "." / "_" / "~"
    /// ALPHA = % x41 - 5A / %x61-7A
    /// DIGIT = % x30 - 39
    /// 
    /// Reference: rfc-7636 https://datatracker.ietf.org/doc/html/rfc7636#section-4.2
    /// </remarks>
    public static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
        return Base64UrlEncoder.Encode(challengeBytes);
    }
}
