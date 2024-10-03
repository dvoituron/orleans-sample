public interface IUrlShortenerGrain : IGrainWithStringKey
{
    Task SetUrl(string value);

    Task<string> GetUrl();
}