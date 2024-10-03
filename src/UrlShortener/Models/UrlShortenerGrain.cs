public class UrlShortenerGrain : Grain, IUrlShortenerGrain
{
    private UrlDetails _urlDetails = UrlDetails.Empty;

    public async Task SetUrl(string value)
    {
        _urlDetails = new UrlDetails
        {
            FullUrl = value,
            ShortenedRouteSegment = this.GetPrimaryKeyString(),
        };

        // await Task.Delay(1000);  // Simulate a delay in setting the URL.
        // DeactivateOnIdle();

        await Task.CompletedTask;
    }

    public Task<string> GetUrl()
    {
        return Task.FromResult(_urlDetails.FullUrl);
    }
}