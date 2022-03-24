namespace Nuages.Identity.CDK;

#nullable disable

public class Data
{
    public string Language { get; set; }
    public string EmailSubject { get; set; }
    public string EmailHtml { get; set; }
}

public class EmailTemplate
{
    public string Key { get; set; }
    public List<Data> Data { get; set; }
}