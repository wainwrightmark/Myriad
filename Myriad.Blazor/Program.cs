using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Myriad;
using MudBlazor.Services;
using TG.Blazor.IndexedDB;

namespace Myriad.Blazor
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
                typeof(Board).Assembly
            )
        );

        builder.RootComponents.Add<App>("#app");

        builder.Services.AddMudServices();

        builder.Services.AddIndexedDB(
            dbStore =>
            {
                dbStore.DbName  = "Myriad";
                dbStore.Version = 1;

                dbStore.Stores.Add(
                    new StoreSchema()
                    {
                        Name = nameof(SavedWord),
                        PrimaryKey =
                            new IndexSpec()
                            {
                                Name    = nameof(SavedWord.uniqueId),
                                Auto    = true,
                                KeyPath = nameof(SavedWord.uniqueId),
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
                            },
                            new ()
                            {
                                Name    = nameof(SavedWord.coordinateString),
                                KeyPath = nameof(SavedWord.coordinateString),
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
