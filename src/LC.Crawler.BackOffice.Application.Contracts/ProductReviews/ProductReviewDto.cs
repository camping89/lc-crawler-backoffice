using System;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.ProductReviews
{
    public class ProductReviewDto : AuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public decimal Rating { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int Likes { get; set; }
        public Guid ProductId { get; set; }

    }
}