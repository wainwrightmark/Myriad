using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using MudBlazor;
using MudBlazor.Services;
using TG.Blazor.IndexedDB;

namespace Moggle.Blazor
{

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddSingleton(
            new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
        );

        builder.Services.AddFluxor(
            options => options.ScanAssemblies(
                typeof(Program).Assembly,
                typeof(MoggleBoard).Assembly
            )
        );

        builder.RootComponents.Add<App>("#app");

        builder.Services.AddMudBlazorDialog();
        builder.Services.AddMudBlazorSnackbar();
        builder.Services.AddMudBlazorResizeListener();

        builder.Services.AddIndexedDB(
            dbStore =>
            {
                dbStore.DbName  = "Moggle";
                dbStore.Version = 1;

                dbStore.Stores.Add(
                    new StoreSchema()
                    {
                        Name = nameof(SavedWord),
                        PrimaryKey =
                            new IndexSpec()
                            {
                                Name    = "uniqueId",
                                Auto    = true,
                                KeyPath = "uniqueId",
                            },
                        Indexes = new List<IndexSpec>()
                        {
                            new()
                            {
                                Name    = nameof(SavedWord.boardId),
                                KeyPath = nameof(SavedWord.boardId),
                                Auto    = false
                            },
                            new()
                            {
                                Name    = nameof(SavedWord.wordText),
                                KeyPath = nameof(SavedWord.wordText),
                                Auto    = false
                            }
                        }
                    }
                );
            }
        );

        var host = builder.Build();

        await host.RunAsync();
    }
}

}
