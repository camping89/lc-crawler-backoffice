using LC.Crawler.BackOffice.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LC.Crawler.BackOffice.CrawlerAccounts;

namespace LC.Crawler.BackOffice.Web.Pages.CrawlerAccounts
{
    public class CreateModalModel : BackOfficePageModel
    {
        [BindProperty]
        public CrawlerAccountCreateDto CrawlerAccount { get; set; }

        private readonly ICrawlerAccountsAppService _crawlerAccountsAppService;

        public CreateModalModel(ICrawlerAccountsAppService crawlerAccountsAppService)
        {
            _crawlerAccountsAppService = crawlerAccountsAppService;
        }

        public async Task OnGetAsync()
        {
            CrawlerAccount = new CrawlerAccountCreateDto();

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            await _crawlerAccountsAppService.CreateAsync(CrawlerAccount);
            return NoContent();
        }
    }
}