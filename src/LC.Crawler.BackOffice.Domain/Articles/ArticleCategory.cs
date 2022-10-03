using System;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.Articles
{
    public class ArticleCategory : Entity
    {

        public Guid ArticleId { get; protected set; }

        public Guid CategoryId { get; protected set; }

        private ArticleCategory()
        {

        }

        public ArticleCategory(Guid articleId, Guid categoryId)
        {
            ArticleId = articleId;
            CategoryId = categoryId;
        }

        public override object[] GetKeys()
        {
            return new object[]
                {
                    ArticleId,
                    CategoryId
                };
        }
    }
}