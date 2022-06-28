using System;
using System.Net.Http;
using System.Threading.Tasks;
using app.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace app
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            // builder.Logging.SetMinimumLevel(LogLevel.Debug);

            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton<ICacher, Cacher>();
            builder.Services.AddSingleton<IAiService, AiService>();
            builder.Services.AddSingleton<IPerfService, PerfService>();
            builder.Services.AddSingleton<IManager, Manager>();
            builder.Services.AddSingleton(s => (IManagerForAi)s.GetRequiredService<IManager>());
            builder.Services.AddSingleton<IMoveService, MoveService>();

            await builder.Build().RunAsync();
        }
    }
}
