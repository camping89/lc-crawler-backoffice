using Volo.Abp.Application.Dtos;
using System;

namespace LC.Crawler.BackOffice.ProductAttributes
{
    public class GetProductAttributesInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string Slug { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public Guid? ProductId { get; set; }

        public GetProductAttributesInput()
        {

        }
    }
}