using Volo.Abp.Application.Dtos;
using System;

namespace LC.Crawler.BackOffice.ArticleComments
{
    public class GetArticleCommentsInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string Name { get; set; }
        public string Content { get; set; }
        public int? LikesMin { get; set; }
        public int? LikesMax { get; set; }
        public DateTime? CreatedAtMin { get; set; }
        public DateTime? CreatedAtMax { get; set; }
        public Guid? ArticleId { get; set; }

        public GetArticleCommentsInput()
        {

        }
    }
}