using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Shared;

namespace LC.Crawler.BackOffice.Web.Pages.DataSources
{
    public class IndexModel : AbpPageModel
    {
        public string UrlFilter { get; set; }
        [SelectItems(nameof(IsActiveBoolFilterItems))]
        public string IsActiveFilter { get; set; }

        public List<SelectListItem> IsActiveBoolFilterItems { get; set; } =
            new List<SelectListItem>
            {
                new SelectListItem("", ""),
                new SelectListItem("Yes", "true"),
                new SelectListItem("No", "false"),
            };
        public string PostToSiteFilter { get; set; }

        private readonly IDataSourcesAppService _dataSourcesAppService;

        public IndexModel(IDataSourcesAppService dataSourcesAppService)
        {
            _dataSourcesAppService = dataSourcesAppService;
        }

        public async Task OnGetAsync()
        {

            await Task.CompletedTask;
        }
    }
}