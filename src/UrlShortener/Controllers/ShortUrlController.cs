using Microsoft.AspNetCore.Mvc;

[ApiController]
public class ShortUrlController : Controller
{
    private IGrainFactory _grains;

    public ShortUrlController(IGrainFactory grains)
    {
        _grains = grains;
    }

    [HttpGet()]
    [Route("shorten")]
    public async Task<IResult> GetShortenAsync([FromQuery] string url)
    {
        var host = $"{Request.Scheme}://{Request.Host.Value}";

        // Validate the URL query string.
        if (string.IsNullOrWhiteSpace(url) &&
            Uri.IsWellFormedUriString(url, UriKind.Absolute) is false)
        {
            return Results.BadRequest($"The URL query string is required and needs to be well formed. Consider, ${host}/api/shorten?url=https://www.microsoft.com.");
        }

        // Create a unique, short ID
        var shortenedRouteSegment = UrlDetails.NewShortUrl;

        // Create and persist a grain with the shortened ID and full URL
        var shortenerGrain = _grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
        await shortenerGrain.SetUrl(url);

        // Return the shortened URL for later use
        var resultBuilder = new UriBuilder(host)
        {
            Path = $"/go/{shortenedRouteSegment}"
        };

        return Results.Ok(resultBuilder.Uri);
    }

    [HttpGet()]
    [Route("go/{shortenedUrl}")]
    public async Task<IResult> GoToUrl(string shortenedUrl)
    {
        // Retrieve the grain using the shortened ID and url to the original URL
        var shortenerGrain = _grains.GetGrain<IUrlShortenerGrain>(shortenedUrl);

        var url = await shortenerGrain.GetUrl();

        // Handles missing schemes, defaults to "http://".
        var redirectBuilder = new UriBuilder(url);

        return Results.Redirect(redirectBuilder.Uri.ToString());
    }
}
