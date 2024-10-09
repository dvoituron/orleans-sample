using Microsoft.AspNetCore.Components;
using Orleans.Streams;

namespace UrlShortener.Components.Pages;

public partial class Home
{
    private List<string> Messages { get; } = new List<string>();

    [Inject]
    public required IClusterClient Client { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var streamProvider = Client.GetStreamProvider(Program.ORLEANS_STREAM_PROVIDER);
            var stream = streamProvider.GetStream<StreamItem>(Program.ORLEANS_STREAM_URL);

            // To solve Build error "Cannot convert lambda expression to type 'IAsyncObserver<StreamItem>'":
            // add "using Orleans.Streams;" at the top of the file
            await stream.SubscribeAsync(async (data, token) =>
            {
                Messages.Add(data.FullUrl);
                await InvokeAsync(StateHasChanged);
            });
        }
    }

    public async Task Button_Click()
    {
        var streamProvider = Client.GetStreamProvider(Program.ORLEANS_STREAM_PROVIDER);
        var stream = streamProvider.GetStream<StreamItem>(Program.ORLEANS_STREAM_URL);

        await stream.OnNextAsync(new StreamItem() { FullUrl = $"New URL {DateTime.Now:HH:mm:ss}" });

        await Task.CompletedTask;
    }
}