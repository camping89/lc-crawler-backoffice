using LC.Crawler.BackOffice.Shared;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using LC.Crawler.BackOffice.CrawlerProxies;

namespace LC.Crawler.BackOffice.Web.Pages.CrawlerProxies
{
    public class EditModalModel : BackOfficePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CrawlerProxyUpdateDto CrawlerProxy { get; set; }

        private readonly ICrawlerProxiesAppService _crawlerProxiesAppService;

        public EditModalModel(ICrawlerProxiesAppService crawlerProxiesAppService)
        {
            _crawlerProxiesAppService = crawlerProxiesAppService;
        }

        public async Task OnGetAsync()
        {
            var crawlerProxy = await _crawlerProxiesAppService.GetAsync(Id);
            CrawlerProxy = ObjectMapper.Map<CrawlerProxyDto, CrawlerProxyUpdateDto>(crawlerProxy);

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            await _crawlerProxiesAppService.UpdateAsync(Id, CrawlerProxy);
            return NoContent();
        }
    }
}