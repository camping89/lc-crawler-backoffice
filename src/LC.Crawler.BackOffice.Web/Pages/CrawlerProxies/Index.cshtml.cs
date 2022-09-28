using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using LC.Crawler.BackOffice.CrawlerProxies;
using LC.Crawler.BackOffice.Shared;

namespace LC.Crawler.BackOffice.Web.Pages.CrawlerProxies
{
    public class IndexModel : AbpPageModel
    {
        public string IpFilter { get; set; }
        public string PortFilter { get; set; }
        public string ProtocolFilter { get; set; }
        public string UsernameFilter { get; set; }
        public string PasswordFilter { get; set; }
        public DateTime? PingedAtFilterMin { get; set; }

        public DateTime? PingedAtFilterMax { get; set; }
        [SelectItems(nameof(IsActiveBoolFilterItems))]
        public string IsActiveFilter { get; set; }

        public List<SelectListItem> IsActiveBoolFilterItems { get; set; } =
            new List<SelectListItem>
            {
                new SelectListItem("", ""),
                new SelectListItem("Yes", "true"),
                new SelectListItem("No", "false"),
            };

        private readonly ICrawlerProxiesAppService _crawlerProxiesAppService;

        public IndexModel(ICrawlerProxiesAppService crawlerProxiesAppService)
        {
            _crawlerProxiesAppService = crawlerProxiesAppService;
        }

        public async Task OnGetAsync()
        {

            await Task.CompletedTask;
        }
    }
}