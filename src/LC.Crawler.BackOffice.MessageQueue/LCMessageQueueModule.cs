using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace LC.Crawler.BackOffice.MessageQueue;

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