using Volo.Abp.Application.Dtos;
using System;

namespace LC.Crawler.BackOffice.Products
{
    public class GetProductsInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string Name { get; set; }
        public string Code { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public int? ExternalIdMin { get; set; }
        public int? ExternalIdMax { get; set; }
        public Guid? FeaturedMediaId { get; set; }
        public Guid? DataSourceId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? MediaId { get; set; }

        public GetProductsInput()
        {

        }
    }
}