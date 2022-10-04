using Volo.Abp.Application.Dtos;
using System;

namespace LC.Crawler.BackOffice.Medias
{
    public class GetMediasInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string Name { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public bool? IsDowloaded { get; set; }

        public GetMediasInput()
        {

        }
    }
}