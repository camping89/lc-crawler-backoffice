using System;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.ProductComments
{
    public class ProductCommentDto : AuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public int Likes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid ProductId { get; set; }

    }
}