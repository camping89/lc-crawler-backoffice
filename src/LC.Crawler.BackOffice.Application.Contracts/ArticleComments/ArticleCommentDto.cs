using System;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.ArticleComments
{
    public class ArticleCommentDto : AuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public int Likes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid ArticleId { get; set; }

    }
}