# 1. Create a new Blazor Server project

1. Open Visual Studio 2022 and create a new project.
   - Select **Blazor Server App** template.
   - Name the project **UrlShortener**.
   - Disable the **Authentication** option.
   - Select **Interactive Render Mode** to **Server**.
   - Select **Interactivity location** to **Global**.
   - Uncheck **Include sample pages**.

2. Add the library **FluentUI Blazor** to the project.
   - Right-click on the project and select **Manage NuGet Packages**.
   - Search for **Microsoft.FluentUI.AspNetCore.Components** and install the latest version.
   - Search for **Microsoft.FluentUI.AspNetCore.Components.Icons** and install the latest version.

3. Go to the `Program.cs` file and add the following code 
   ```csharp
   // Add FluentUI Blazor services
   builder.Services.AddFluentUIComponents();
   builder.Services.AddHttpClient();
   ```

4. Add this style in the `app.css` file:
   ```css
   body {
     margin: 0;
     padding: 0;
     height: 100vh;
     font-family: var(--body-font);
     font-size: var(--type-ramp-base-font-size);
     line-height: var(--type-ramp-base-line-height);
     font-weight: var(--font-weight);
     color: var(--neutral-foreground-rest);
     background: var(--neutral-fill-layer-rest);
     overflow: hidden;
   }
   ```
5. Add this `using` statement in the `_Imports.razor` file:
   ```csharp
   @using Microsoft.FluentUI.AspNetCore.Components
   ```

6. Replace the content of the `MainLayout.razor` file with the following code:
   ```html
   @inherits LayoutComponentBase
   
   <FluentLayout>
       <FluentHeader>
           URL Shortener - .NET Orleans
       </FluentHeader>
   
       <FluentStack Orientation="Orientation.Horizontal" Width="100%" Style="height: calc(100vh - 84px);">
           <FluentNavMenu Width="250">
               <FluentNavLink Icon="@(new Icons.Regular.Size24.Home())" Href="/">Home</FluentNavLink>
           </FluentNavMenu>
   
           <FluentBodyContent Style="overflow-y: auto;">
               @Body
           </FluentBodyContent>
       </FluentStack>
   
       <FluentFooter Style="background: var(--neutral-layer-4); padding: 10px;">
           Denis Voituron - Demo 2024
       </FluentFooter>
   </FluentLayout>
   
   <div id="blazor-error-ui">
       An unhandled error has occurred.
       <a href="" class="reload">Reload</a>
       <a class="dismiss">🗙</a>
   </div>
   
   <FluentToastProvider />
   <FluentDialogProvider />
   <FluentTooltipProvider />
   <FluentMessageBarProvider />
   <FluentMenuProvider />
   ```

# 2. Create an API to shorten URLs

1. Add a new folder named **Controllers** in the project.

2. Add a new controller named **ShortUrlController.cs** in the **Controllers** folder:
   
   ```csharp
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
   ```

3. Update the `Program.cs` file to add the API controller:

   ```csharp
   // Add Controllers
   builder.Services.AddControllers();
   ```

   And to map the API controller:
   ```csharp
   app.MapControllers();
   ```

4. Run the application and test the API by navigating 
   to `https://localhost:7288/shorten?url=https://www.microsoft.com`.
   You will receive an new shortened URL like `https://localhost:7288/go/TODO`.

# 3. Create the first Grain.

1. Add the library **Microsoft.Orleans.Server** to the project.
   - Right-click on the project and select **Manage NuGet Packages**.
   - Search for **Microsoft.Orleans.Server** and install the latest version.

2. Inject the **Orleans** services in `Program.cs` file:

   ```csharp
   // Add Orleans services
   builder.Host.UseOrleans(siloBuilder =>
   {
       siloBuilder.UseLocalhostClustering();
   });
   ```

3. Create a new folder named **Models** in the project

4. Add this interface in the `IUrlShortenerGrain.cs` file:

   ```csharp
   public interface IUrlShortenerGrain : IGrainWithStringKey
   {
       Task SetUrl(string value);
   
       Task<string> GetUrl();
   }
   ```

   > We are using the `IGrainWithStringKey` interface to define the primary key of the grain,
   > as a string representing the shortened URL.

5. Add this class in the `UrlShortenerGrain.cs` file:
   ```csharp
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
   ```

6. Add this constructor to the `ShortUrlController` to have a reference to the **GrainFactory**:

   ```csharp
   private IGrainFactory _grains;

   public ShortUrlController(IGrainFactory grains)
   {
       _grains = grains;
   }
   ```

7. Update the `GetShortenAsync` method to use the **Grain**:
   ```csharp
   // Create a unique, short ID
   var shortenedRouteSegment = UrlDetails.NewShortUrl;

   // Create and persist a grain with the shortened ID and full URL
   var shortenerGrain = _grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
   await shortenerGrain.SetUrl(url);
   ```

8. Run the application and test the API by navigating 
   to `https://localhost:7288/shorten?url=https://www.microsoft.com`.
   You will receive an new shortened URL like `https://localhost:7288/go/67C2BC0F`.

   > You can use the `Samples.http` file to test the API with Visual Studio.

# 4. Add the GoToUrl controller

1. Update the controller adding the `GoToUrl` method:

   ```csharp
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
   ```

2. Run the application and test the API by navigating 
   to `https://localhost:7288/shorten?url=https://www.microsoft.com`
   and next to the URL returned by the API.

   You will be redirected to `https://www.microsoft.com`.

# 5. Add Orleans Dashboard

1. Add the library **OrleansDashboard** to the project.
   - Right-click on the project and select **Manage NuGet Packages**.
   - Search for **OrleansDashboard** and install the latest version.

2. Inject the **Orleans Dashboard** services in `Program.cs` file:

   ```csharp
   builder.Host.UseOrleans(siloBuilder =>
   {
       siloBuilder.UseLocalhostClustering();
       siloBuilder.UseDashboard();
       });
   ```

3. Run the application and navigate to `http://localhost:8080/` to display the **Orleans Dashboard**.

# 6. Manage the Grain Timeout

1. Update the `Program.cs` using this code:
   ```csharp
   // Set DeactivationTimeout to 1 minute
   siloBuilder.Configure<GrainCollectionOptions>(options =>
   {
       options.CollectionQuantum = TimeSpan.FromSeconds(1);    // Garbage Collector Ticks
       options.CollectionAge = TimeSpan.FromSeconds(5);        // Time to wait before deactivating a Grain
   });
   ```

2. Run the application and test the API by navigating 
   to `https://localhost:7288/shorten?url=https://www.microsoft.com`
   
3. Navigate to http://localhost:8080/ and check the **Grains** tab.
   After 5 seconds, the grain will be deactivated.

   > By default a Grain is deactivated after 2 hours of idle.

4. You could also deactivate a grain manually by calling the `DeactivateOnIdle` method,
   in the `UrlShortenerGrain.SetUrl(string value)` method:
   ```csharp
   DeactivateOnIdle();
   ```

# 7. Add Persistence to the Grain

1. We will use the Azure Blob Storage to persist the grain state. You nedd to install the following tools:
   - [Azurite Storage Emulator](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) to emulate the Azure Blob Storage.
   - [Azure Storage Explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer/) to manage and read the Azure Blob Storage.

   > Azurite is automatically available with Visual Studio 2022. 
   > If you're running an earlier version of Visual Studio, you can install Azurite by using either 
   > Node Package Manager (npm), DockerHub, or by cloning the Azurite GitHub repository.
   > Navigate to the appropriate location and start `azurite.exe`. 
   > After you run the executable file, **Azurite** listens for connections.

   Once installed, launch the Azurite emulator with the following command:
   ```shell
   "C:\Program Files\Microsoft Visual Studio\2022\Preview\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator\Azurite.exe" --location .\Azurite-Data
   ```

   > The `--location` parameter specifies the location of the data files.

2. Add the library **Microsoft.Orleans.Persistence.AzureStorage** to the project.

3. Update the `Program.cs` file to inject the **Azure Blob Storage** services:
   ```csharp
   public const string ORLEANS_STORAGE_NAME = "urls";
   ```
   ```csharp
   siloBuilder.AddMemoryGrainStorage(ORLEANS_STORAGE_NAME);

   siloBuilder.AddAzureBlobGrainStorage(
       name: ORLEANS_STORAGE_NAME,   // Orleans Storage Name
       options =>
       { 
           options.ContainerName = "urls";     // Azure Blob Container Name
           options.BlobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
       });
   ```

4. Update the `UrlShortenerGrain` to add this constructor:

   ```csharp
   private readonly IPersistentState<UrlDetails> _urlDetails;

   public UrlShortenerGrain(
       [PersistentState(stateName: "url", storageName: Program.ORLEANS_STORAGE_NAME)] IPersistentState<UrlDetails> urlDetails)
   {
       _urlDetails = urlDetails;
   }
   ```

   Replace `_urlDetails` by `_urlDetails.State` in the `SetUrl` and `GetUrl` methods.

   Add this line in the `SetUrl` method to persist the grain state:
   ```csharp
   await _urlDetails.WriteStateAsync();
   ```

5. Grain type names

   Orleans creates a grain type name for you based on your grain implementation class by removing the suffix "Grain" 
   from the class name, if it's present, and converting the resulting string into its lower-case representation. 
   For example, a class named `ShoppingCartGrain` will be given the grain type name `shoppingcart`.

   You can override the default grain type name by using the `[GrainType]` attribute:
   ```csharp
   [GrainType("shortener")]
   ```

   This grain type name is used to generate the Azure Blob Storage key: `{State_Name}-{Grain_Type_Name}`.