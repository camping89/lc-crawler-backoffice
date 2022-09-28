using LC.Crawler.BackOffice.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace LC.Crawler.BackOffice.Web.Pages;

public abstract class BackOfficePageModel : AbpPageModel
{
    protected BackOfficePageModel()
    {
        LocalizationResourceType = typeof(BackOfficeResource);
    }
}
