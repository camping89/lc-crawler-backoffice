using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.ArticleComments
{
    public class ArticleCommentUpdateDto
    {
        [Required]
        public string Name { get; set; }
        public string Content { get; set; }
        public int Likes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid ArticleId { get; set; }

    }
}