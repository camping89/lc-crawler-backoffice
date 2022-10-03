using Volo.Abp.Application.Dtos;
using System;

namespace LC.Crawler.BackOffice.Products
{
    public class GetProductsInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string Name { get; set; }
        public string Brand { get; set; }
        public double? RatingMin { get; set; }
        public double? RatingMax { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public double? DiscountPercentMin { get; set; }
        public double? DiscountPercentMax { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? MediaId { get; set; }

        public GetProductsInput()
        {

        }
    }
}