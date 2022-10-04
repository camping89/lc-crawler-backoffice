using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ProductComments
{
    public class ProductCommentUpdateDto
    {
        [Required]
        public string Name { get; set; }
        public string Content { get; set; }
        public int Likes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid ProductId { get; set; }

    }
}