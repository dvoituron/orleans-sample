[GenerateSerializer()]
[Alias("UrlDetails")]
public record class UrlDetails
{
    [Id(0)]
    public required string FullUrl { get; set; }

    [Id(1)]
    public required string ShortenedRouteSegment { get; set; }

    public static UrlDetails Empty => new UrlDetails
    {
        FullUrl = string.Empty,
        ShortenedRouteSegment = string.Empty
    };

    public static string NewShortUrl => Guid.NewGuid().GetHashCode().ToString("X");
}