using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.Articles;

public class ArticleResultDto : AuditedEntityDto<Guid>
{
    public string Title { get; set; }
    public string Excerpt { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Author { get; set; }
    public List<string> Tags { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public string FeatureImageUrl { get; set; }
}