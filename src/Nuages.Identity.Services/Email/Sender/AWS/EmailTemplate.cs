namespace Nuages.Identity.Services.Email.Sender.AWS;

#nullable disable

// ReSharper disable once ClassNeverInstantiated.Global
public class Data
{
    public string Language { get; set; }
    public string EmailSubject { get; set; }
    public string EmailHtml { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class EmailTemplate
{
    public string Key { get; set; }
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<Data> Data { get; set; }
}