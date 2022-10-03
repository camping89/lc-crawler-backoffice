using LC.Crawler.BackOffice.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LC.Crawler.BackOffice.Medias;

namespace LC.Crawler.BackOffice.Web.Pages.Medias
{
    public class CreateModalModel : BackOfficePageModel
    {
        [BindProperty]
        public MediaCreateDto Media { get; set; }

        private readonly IMediasAppService _mediasAppService;

        public CreateModalModel(IMediasAppService mediasAppService)
        {
            _mediasAppService = mediasAppService;
        }

        public async Task OnGetAsync()
        {
            Media = new MediaCreateDto();

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            await _mediasAppService.CreateAsync(Media);
            return NoContent();
        }
    }
}