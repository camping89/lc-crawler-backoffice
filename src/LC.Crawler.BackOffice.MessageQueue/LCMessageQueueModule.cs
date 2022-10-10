using Volo.Abp.AutoMapper;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;

namespace LC.Crawler.BackOffice.MessageQueue;


[DependsOn(typeof(AbpEventBusRabbitMqModule))]
public class LCMessageQueueModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<LCMessageQueueModule>();
        });
    }
}