using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Shared;

namespace LC.Crawler.BackOffice.Web.Pages.Medias
{
    public class IndexModel : AbpPageModel
    {
        public string NameFilter { get; set; }
        public string ContentTypeFilter { get; set; }
        public string UrlFilter { get; set; }

        private readonly IMediasAppService _mediasAppService;

        public IndexModel(IMediasAppService mediasAppService)
        {
            _mediasAppService = mediasAppService;
        }

        public async Task OnGetAsync()
        {

            await Task.CompletedTask;
        }
    }
}