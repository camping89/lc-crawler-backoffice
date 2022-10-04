using System;
using System.Collections.Generic;
using LC.Crawler.BackOffice.Articles;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.ArticleComments;

public class ArticleCommentResultDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string Content { get; set; }
    public int Likes { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class ArticleCommentsResultDto
{
    public string DataSource { get; set; }
    public ArticleResultDto Article { get; set; }
    public List<ArticleCommentResultDto> Comments { get; set; }
}