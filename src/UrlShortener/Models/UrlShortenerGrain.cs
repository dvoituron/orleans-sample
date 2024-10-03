public class UrlShortenerGrain : Grain, IUrlShortenerGrain
{
    private UrlDetails _urlDetails = UrlDetails.Empty;

    public Task SetUrl(string value)
    {
        _urlDetails = new UrlDetails
        {
            FullUrl = value,
            ShortenedRouteSegment = this.GetPrimaryKeyString(),
        };

        return Task.CompletedTask;
    }

    public Task<string> GetUrl()
    {
        return Task.FromResult(_urlDetails.FullUrl);
    }
}