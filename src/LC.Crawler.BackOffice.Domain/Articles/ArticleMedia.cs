using System;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.Articles
{
    public class ArticleMedia : Entity
    {

        public Guid ArticleId { get; protected set; }

        public Guid MediaId { get; protected set; }

        private ArticleMedia()
        {

        }

        public ArticleMedia(Guid articleId, Guid mediaId)
        {
            ArticleId = articleId;
            MediaId = mediaId;
        }

        public override object[] GetKeys()
        {
            return new object[]
                {
                    ArticleId,
                    MediaId
                };
        }
    }
}