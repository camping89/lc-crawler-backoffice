using LC.Crawler.BackOffice.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace LC.Crawler.BackOffice.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class BackOfficeController : AbpControllerBase
{
    protected BackOfficeController()
    {
        LocalizationResource = typeof(BackOfficeResource);
    }
}
