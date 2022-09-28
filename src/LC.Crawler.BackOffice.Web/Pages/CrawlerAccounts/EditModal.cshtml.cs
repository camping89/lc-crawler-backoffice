using LC.Crawler.BackOffice.Shared;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using LC.Crawler.BackOffice.CrawlerAccounts;

namespace LC.Crawler.BackOffice.Web.Pages.CrawlerAccounts
{
    public class EditModalModel : BackOfficePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CrawlerAccountUpdateDto CrawlerAccount { get; set; }

        private readonly ICrawlerAccountsAppService _crawlerAccountsAppService;

        public EditModalModel(ICrawlerAccountsAppService crawlerAccountsAppService)
        {
            _crawlerAccountsAppService = crawlerAccountsAppService;
        }

        public async Task OnGetAsync()
        {
            var crawlerAccount = await _crawlerAccountsAppService.GetAsync(Id);
            CrawlerAccount = ObjectMapper.Map<CrawlerAccountDto, CrawlerAccountUpdateDto>(crawlerAccount);

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            await _crawlerAccountsAppService.UpdateAsync(Id, CrawlerAccount);
            return NoContent();
        }
    }
}