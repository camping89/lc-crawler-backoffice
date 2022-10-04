using Volo.Abp.Application.Dtos;
using System;

namespace LC.Crawler.BackOffice.Articles
{
    public class GetArticlesInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string Title { get; set; }
        public string Excerpt { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedAtMin { get; set; }
        public DateTime? CreatedAtMax { get; set; }
        public string Author { get; set; }
        public string Tags { get; set; }
        public int? LikeCountMin { get; set; }
        public int? LikeCountMax { get; set; }
        public int? CommentCountMin { get; set; }
        public int? CommentCountMax { get; set; }
        public int? ShareCountMin { get; set; }
        public int? ShareCountMax { get; set; }
        public Guid? FeaturedMediaId { get; set; }
        public Guid? DataSourceId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? MediaId { get; set; }

        public GetArticlesInput()
        {

        }
    }
}