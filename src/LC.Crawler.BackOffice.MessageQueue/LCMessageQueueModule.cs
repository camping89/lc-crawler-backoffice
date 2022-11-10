using LC.Crawler.BackOffice.MessageQueue.BackgroundWorkers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BackgroundWorkers.Hangfire;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;

namespace LC.Crawler.BackOffice.MessageQueue;


[DependsOn(
    typeof(AbpEventBusRabbitMqModule))]
public class LCMessageQueueModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<LCMessageQueueModule>();
        });
    }
    
    public override Task OnApplicationInitializationAsync(
        ApplicationInitializationContext context)
    {
        ConfigBackgroundWorker(context);
        return Task.CompletedTask;
    }
    
    private Task ConfigBackgroundWorker(ApplicationInitializationContext context)
    {
        var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (hostEnvironment.IsProduction())
        {
            // Process download and save image
            context.AddBackgroundWorkerAsync<PushDatasourceBackgroundWorker>();
        }
        else
        {
            // Process download and save image
            context.AddBackgroundWorkerAsync<PushDatasourceBackgroundWorker>();
        }
        
        return Task.CompletedTask;
    }

}