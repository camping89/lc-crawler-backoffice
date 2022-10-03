using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.Products
{
    public class ProductCreateDto
    {
        [Required]
        public string Name { get; set; }
        public string Brand { get; set; }
        public double Rating { get; set; }
        public decimal Price { get; set; }
        public double DiscountPercent { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public List<Guid> CategoryIds { get; set; }
        public List<Guid> MediaIds { get; set; }
    }
}