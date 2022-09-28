using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using LC.Crawler.BackOffice.CrawlerCredentials;
using LC.Crawler.BackOffice.Enums;

namespace LC.Crawler.BackOffice.Web.Pages.CrawlerCredentials
{
    public class IndexModel : AbpPageModel
    {
        public DataSourceType? DataSourceTypeFilter { get; set; }
        public DateTime? CrawledAtFilterMin { get; set; }

        public DateTime? CrawledAtFilterMax { get; set; }
        [SelectItems(nameof(IsAvailableBoolFilterItems))]
        public string IsAvailableFilter { get; set; }

        public List<SelectListItem> IsAvailableBoolFilterItems { get; set; } =
            new List<SelectListItem>
            {
                new SelectListItem("", ""),
                new SelectListItem("Yes", "true"),
                new SelectListItem("No", "false"),
            };

        private readonly ICrawlerCredentialsAppService _crawlerCredentialsAppService;

        public IndexModel(ICrawlerCredentialsAppService crawlerCredentialsAppService)
        {
            _crawlerCredentialsAppService = crawlerCredentialsAppService;
        }

        public async Task OnGetAsync()
        {

            await Task.CompletedTask;
        }
    }
}