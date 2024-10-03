public record class UrlDetails
{
    public required string FullUrl { get; set; }
    
    public required string ShortenedRouteSegment { get; set; }

    public static UrlDetails Empty => new UrlDetails
    {
        FullUrl = string.Empty,
        ShortenedRouteSegment = string.Empty
    };

    public static string NewShortUrl => Guid.NewGuid().GetHashCode().ToString("X");
}