using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ProductReviews
{
    public class ProductReviewUpdateDto
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public decimal Rating { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int Likes { get; set; }
        public Guid ProductId { get; set; }

    }
}