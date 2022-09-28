using LC.Crawler.BackOffice.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LC.Crawler.BackOffice.CrawlerCredentials;

namespace LC.Crawler.BackOffice.Web.Pages.CrawlerCredentials
{
    public class CreateModalModel : BackOfficePageModel
    {
        [BindProperty]
        public CrawlerCredentialCreateDto CrawlerCredential { get; set; }

        private readonly ICrawlerCredentialsAppService _crawlerCredentialsAppService;

        public CreateModalModel(ICrawlerCredentialsAppService crawlerCredentialsAppService)
        {
            _crawlerCredentialsAppService = crawlerCredentialsAppService;
        }

        public async Task OnGetAsync()
        {
            CrawlerCredential = new CrawlerCredentialCreateDto();

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            await _crawlerCredentialsAppService.CreateAsync(CrawlerCredential);
            return NoContent();
        }
    }
}