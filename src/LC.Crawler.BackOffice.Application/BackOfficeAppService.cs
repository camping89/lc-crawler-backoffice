using LC.Crawler.BackOffice.Localization;
using Volo.Abp.Application.Services;

namespace LC.Crawler.BackOffice;

/* Inherit your application services from this class.
 */
public abstract class BackOfficeAppService : ApplicationService
{
    protected BackOfficeAppService()
    {
        LocalizationResource = typeof(BackOfficeResource);
    }
}
