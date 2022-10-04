using Volo.Abp.Application.Dtos;
using System;

namespace LC.Crawler.BackOffice.ProductReviews
{
    public class GetProductReviewsInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string Name { get; set; }
        public string Content { get; set; }
        public decimal? RatingMin { get; set; }
        public decimal? RatingMax { get; set; }
        public DateTime? CreatedAtMin { get; set; }
        public DateTime? CreatedAtMax { get; set; }
        public int? LikesMin { get; set; }
        public int? LikesMax { get; set; }
        public Guid? ProductId { get; set; }

        public GetProductReviewsInput()
        {

        }
    }
}