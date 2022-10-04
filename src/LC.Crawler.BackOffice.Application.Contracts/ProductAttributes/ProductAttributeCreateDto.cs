using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ProductAttributes
{
    public class ProductAttributeCreateDto
    {
        public string Slug { get; set; }
        [Required]
        public string Key { get; set; }
        [Required]
        public string Value { get; set; }
        public Guid ProductId { get; set; }
    }
}