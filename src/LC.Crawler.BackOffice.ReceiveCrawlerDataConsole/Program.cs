using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Volo.Abp;

namespace LC.Crawler.BackOffice.ReceiveCrawlerDataConsole;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File($"Logs/{DateTime.UtcNow:dd-MM-yyyy}logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            Log.Information("Starting console host.");

            // await Host.CreateDefaultBuilder(args)
            //     .ConfigureServices(services =>
            //     {
            //         services.AddHostedService<ReceiveCrawlerDataConsoleHostedService>();
            //     })
            //     .UseAutofac().UseSerilog()
            //     .RunConsoleAsync();
            //
            // return 0;
            var builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureServices(services =>
            {
                services.AddHostedService<ReceiveCrawlerDataConsoleHostedService>();
                services.AddApplicationAsync<ReceiveCrawlerDataConsoleModule>(options =>
                {
                    options.Services.ReplaceConfiguration(services.GetConfiguration());
                    options.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
                });
            }).UseAutofac().UseConsoleLifetime();

            var host = builder.Build();
            await host.Services.GetRequiredService<IAbpApplicationWithExternalServiceProvider>().InitializeAsync(host.Services);

            await host.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
