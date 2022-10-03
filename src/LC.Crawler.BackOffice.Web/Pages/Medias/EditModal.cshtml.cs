using LC.Crawler.BackOffice.Shared;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using LC.Crawler.BackOffice.Medias;

namespace LC.Crawler.BackOffice.Web.Pages.Medias
{
    public class EditModalModel : BackOfficePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public MediaUpdateDto Media { get; set; }

        private readonly IMediasAppService _mediasAppService;

        public EditModalModel(IMediasAppService mediasAppService)
        {
            _mediasAppService = mediasAppService;
        }

        public async Task OnGetAsync()
        {
            var media = await _mediasAppService.GetAsync(Id);
            Media = ObjectMapper.Map<MediaDto, MediaUpdateDto>(media);

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            await _mediasAppService.UpdateAsync(Id, Media);
            return NoContent();
        }
    }
}