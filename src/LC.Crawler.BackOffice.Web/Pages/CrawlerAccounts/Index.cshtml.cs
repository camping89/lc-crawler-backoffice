using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using LC.Crawler.BackOffice.CrawlerAccounts;
using LC.Crawler.BackOffice.Enums;

namespace LC.Crawler.BackOffice.Web.Pages.CrawlerAccounts
{
    public class IndexModel : AbpPageModel
    {
        public string UsernameFilter { get; set; }
        public string PasswordFilter { get; set; }
        public string TwoFactorCodeFilter { get; set; }
        public AccountType? AccountTypeFilter { get; set; }
        public AccountStatus? AccountStatusFilter { get; set; }
        public string EmailFilter { get; set; }
        public string EmailPasswordFilter { get; set; }
        [SelectItems(nameof(IsActiveBoolFilterItems))]
        public string IsActiveFilter { get; set; }

        public List<SelectListItem> IsActiveBoolFilterItems { get; set; } =
            new List<SelectListItem>
            {
                new SelectListItem("", ""),
                new SelectListItem("Yes", "true"),
                new SelectListItem("No", "false"),
            };

        private readonly ICrawlerAccountsAppService _crawlerAccountsAppService;

        public IndexModel(ICrawlerAccountsAppService crawlerAccountsAppService)
        {
            _crawlerAccountsAppService = crawlerAccountsAppService;
        }

        public async Task OnGetAsync()
        {

            await Task.CompletedTask;
        }
    }
}