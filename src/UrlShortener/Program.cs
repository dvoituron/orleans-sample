using Azure.Storage.Blobs;
using Microsoft.FluentUI.AspNetCore.Components;
using Orleans.Configuration;
using UrlShortener.Components;

namespace UrlShortener
{
    public class Program
    {
        public const string ORLEANS_STORAGE_NAME = "urls";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                            .AddInteractiveServerComponents();

            // Add FluentUI Blazor services
            builder.Services.AddFluentUIComponents();
            builder.Services.AddHttpClient();

            // Add Controllers
            builder.Services.AddControllers();

            // Add Orleans services
            builder.Host.UseOrleans(siloBuilder =>
            {
                siloBuilder.UseLocalhostClustering();
                siloBuilder.UseDashboard();

                // Add Grain Storage
                siloBuilder.AddMemoryGrainStorage(ORLEANS_STORAGE_NAME);
                siloBuilder.AddAzureBlobGrainStorage(
                    name: ORLEANS_STORAGE_NAME,   // Orleans Storage Name
                    options =>
                    {
                        options.ContainerName = "urls";     // Azure Blob Container Name
                        options.BlobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
                    });

                // Set DeactivationTimeout to 1 minute
                siloBuilder.Configure<GrainCollectionOptions>(options =>
                {
                    options.CollectionQuantum = TimeSpan.FromSeconds(1);    // Garbage Collector Ticks
                    options.CollectionAge = TimeSpan.FromSeconds(5);        // Time to wait before deactivating a Grain
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapControllers();

            app.MapRazorComponents<App>()
               .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
