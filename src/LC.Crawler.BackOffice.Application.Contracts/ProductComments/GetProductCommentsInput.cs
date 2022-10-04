using Volo.Abp.Application.Dtos;
using System;

namespace LC.Crawler.BackOffice.ProductComments
{
    public class GetProductCommentsInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string Name { get; set; }
        public string Content { get; set; }
        public int? LikesMin { get; set; }
        public int? LikesMax { get; set; }
        public DateTime? CreatedAtMin { get; set; }
        public DateTime? CreatedAtMax { get; set; }
        public Guid? ProductId { get; set; }

        public GetProductCommentsInput()
        {

        }
    }
}