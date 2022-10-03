using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.Articles
{
    public class ArticleCreateDto
    {
        [Required]
        public string Title { get; set; }
        public string Excerpt { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Author { get; set; }
        public string Tags { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
        public List<Guid> CategoryIds { get; set; }
    }
}