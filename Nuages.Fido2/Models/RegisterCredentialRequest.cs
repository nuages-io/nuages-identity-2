namespace Nuages.Fido2.Models;


public class RegisterCredentialRequest
{
    public string Id { get; set; } = string.Empty;
    public string RawId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Response Response { get; set; } = new();
}

public class Response
{
    public string AttestationObject { get; set; } = string.Empty;
    public string ClientDataJson { get; set; } = string.Empty;
}

