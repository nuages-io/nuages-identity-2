namespace Nuages.Identity.Services.AspNetIdentity;

public class PasswordHistory
{
    public List<string> Passwords { get; set; } = new();


    public void AddPassword(string hash, int keepPasspordCount)
    {
        var list = new List<string>();
        list.AddRange(Passwords);
        list.Insert(0, hash + "|" + DateTime.UtcNow.ToString("O"));
        
        Passwords = list.Take(keepPasspordCount).ToList();
    }
}