using System.Threading.Tasks;
using LC.Crawler.BackOffice.MongoDB;
using LC.Crawler.BackOffice.PageDatasource;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LC.Crawler.BackOffice.DevConsole;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(BackOfficeDomainModule),
    typeof(BackOfficeDomainSharedModule),
    typeof(BackOfficeMongoDbModule),
    typeof(PageDataSourceMongoDbModule)
)]
public class DevConsoleModule : AbpModule
{
    public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<DevConsoleModule>>();
        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
        logger.LogInformation($"MySettingName => {configuration["MySettingName"]}");

        var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();
        logger.LogInformation($"EnvironmentName => {hostEnvironment.EnvironmentName}");

        return Task.CompletedTask;
    }
}
