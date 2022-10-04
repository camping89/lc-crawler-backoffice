using System;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.Medias
{
    public class MediaDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public bool IsDowloaded { get; set; }

    }
}