using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.CrawlerProxies;
using LC.Crawler.BackOffice.CrawlerAccounts;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using LC.Crawler.BackOffice.CrawlerCredentials;

namespace LC.Crawler.BackOffice.Web.Pages.CrawlerCredentials
{
    public class EditModalModel : BackOfficePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CrawlerCredentialUpdateDto CrawlerCredential { get; set; }

        public CrawlerAccountDto CrawlerAccount { get; set; }
        public CrawlerProxyDto CrawlerProxy { get; set; }

        private readonly ICrawlerCredentialsAppService _crawlerCredentialsAppService;

        public EditModalModel(ICrawlerCredentialsAppService crawlerCredentialsAppService)
        {
            _crawlerCredentialsAppService = crawlerCredentialsAppService;
        }

        public async Task OnGetAsync()
        {
            var crawlerCredentialWithNavigationPropertiesDto = await _crawlerCredentialsAppService.GetWithNavigationPropertiesAsync(Id);
            CrawlerCredential = ObjectMapper.Map<CrawlerCredentialDto, CrawlerCredentialUpdateDto>(crawlerCredentialWithNavigationPropertiesDto.CrawlerCredential);

            CrawlerAccount = crawlerCredentialWithNavigationPropertiesDto.CrawlerAccount;
            CrawlerProxy = crawlerCredentialWithNavigationPropertiesDto.CrawlerProxy;

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            await _crawlerCredentialsAppService.UpdateAsync(Id, CrawlerCredential);
            return NoContent();
        }
    }
}