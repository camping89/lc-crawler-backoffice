using Volo.Abp.Application.Dtos;
using System;

namespace LC.Crawler.BackOffice.ProductVariants
{
    public class GetProductVariantsInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string SKU { get; set; }
        public decimal? RetailPriceMin { get; set; }
        public decimal? RetailPriceMax { get; set; }
        public double? DiscountRateMin { get; set; }
        public double? DiscountRateMax { get; set; }
        public decimal? DiscountedPriceMin { get; set; }
        public decimal? DiscountedPriceMax { get; set; }
        public Guid? ProductId { get; set; }

        public GetProductVariantsInput()
        {

        }
    }
}