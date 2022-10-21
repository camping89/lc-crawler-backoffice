using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.Articles
{
    public class ArticleUpdateDto : IHasConcurrencyStamp
    {
        [Required]
        public string Title { get;                 set; }
        public string       Excerpt         { get; set; }
        public string       Content         { get; set; }
        public DateTime     CreatedAt       { get; set; }
        public string       Author          { get; set; }
        public List<string> Tags            { get; set; }
        public int          LikeCount       { get; set; }
        public int          CommentCount    { get; set; }
        public int          ShareCount      { get; set; }
        public Guid?        FeaturedMediaId { get; set; }
        public Guid         DataSourceId    { get; set; }
        public List<Guid>   CategoryIds     { get; set; }
        public List<Guid>   MediaIds        { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}