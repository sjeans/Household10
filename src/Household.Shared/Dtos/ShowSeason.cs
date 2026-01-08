namespace Household.Shared.Dtos;

public class ShowSeason
{
    public int id { get; set; }
    public string url { get; set; } = string.Empty;
    public int number { get; set; }
    public string name { get; set; } = string.Empty;
    public int episodeOrder { get; set; }
    public string premiereDate { get; set; } = string.Empty;
    public string endDate { get; set; } = string.Empty;
    public object? network { get; set; }
    public Webchannel webChannel { get; set; } = new ();
    public Image image { get; set; } = new ();
    public string summary { get; set; } = string.Empty;
    public Links _links { get; set; } = new();
}

public class Webchannel
{
    public int id { get; set; }
    public string name { get; set; } = string.Empty;
    public object? country { get; set; }
    public string officialSite { get; set; } = string.Empty;
}

public class Image
{
    public string medium { get; set; } = string.Empty;
    public string original { get; set; } = string.Empty;
}

public class Links
{
    public Self self { get; set; } = new();
}

public class Self
{
    public string href { get; set; } = string.Empty;
}
