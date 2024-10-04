﻿using UrlShortener;

[GrainType("shortener")]
public class UrlShortenerGrain : Grain, IUrlShortenerGrain
{
    private readonly IPersistentState<UrlDetails> _urlDetails;

    public UrlShortenerGrain(
        [PersistentState(stateName: "url", storageName: Program.ORLEANS_STORAGE_NAME)] IPersistentState<UrlDetails> urlDetails)
    {
        _urlDetails = urlDetails;
    }

    public async Task SetUrl(string value)
    {
        _urlDetails.State = new UrlDetails
        {
            FullUrl = value,
            ShortenedRouteSegment = this.GetPrimaryKeyString(),
        };

        // await Task.Delay(1000);  // Simulate a delay in setting the URL.
        // DeactivateOnIdle();

        await _urlDetails.WriteStateAsync();

        await Task.CompletedTask;
    }

    public Task<string> GetUrl()
    {
        return Task.FromResult(_urlDetails.State.FullUrl);
    }
}