using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Fluxor;
using MudBlazor;
using MudBlazor.Services;

namespace Moggle.Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });

            builder.Services.AddFluxor(options => options.ScanAssemblies(typeof(Program).Assembly, typeof(MoggleBoard).Assembly));

            builder.RootComponents.Add<App>("#app");

            builder.Services.AddMudBlazorDialog();
            builder.Services.AddMudBlazorSnackbar();
            builder.Services.AddMudBlazorResizeListener();

            builder.Services.AddBlazoredLocalStorage();


            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
