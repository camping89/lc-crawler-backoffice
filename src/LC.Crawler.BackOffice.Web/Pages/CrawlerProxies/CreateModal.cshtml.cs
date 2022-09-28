using LC.Crawler.BackOffice.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LC.Crawler.BackOffice.CrawlerProxies;

namespace LC.Crawler.BackOffice.Web.Pages.CrawlerProxies
{
    public class CreateModalModel : BackOfficePageModel
    {
        [BindProperty]
        public CrawlerProxyCreateDto CrawlerProxy { get; set; }

        private readonly ICrawlerProxiesAppService _crawlerProxiesAppService;

        public CreateModalModel(ICrawlerProxiesAppService crawlerProxiesAppService)
        {
            _crawlerProxiesAppService = crawlerProxiesAppService;
        }

        public async Task OnGetAsync()
        {
            CrawlerProxy = new CrawlerProxyCreateDto();

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            await _crawlerProxiesAppService.CreateAsync(CrawlerProxy);
            return NoContent();
        }
    }
}