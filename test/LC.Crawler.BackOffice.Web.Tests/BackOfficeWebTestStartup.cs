using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LC.Crawler.BackOffice;

public class BackOfficeWebTestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplication<BackOfficeWebTestModule>();
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        app.InitializeApplication();
    }
}
