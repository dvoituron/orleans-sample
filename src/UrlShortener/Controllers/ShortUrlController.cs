using Microsoft.AspNetCore.Mvc;

[ApiController]
public class ShortUrlController : Controller
{
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
        var shortenedRouteSegment = $"TODO";
        await Task.CompletedTask;

        // Return the shortened URL for later use
        var resultBuilder = new UriBuilder(host)
        {
            Path = $"/go/{shortenedRouteSegment}"
        };

        return Results.Ok(resultBuilder.Uri);
    }
}
