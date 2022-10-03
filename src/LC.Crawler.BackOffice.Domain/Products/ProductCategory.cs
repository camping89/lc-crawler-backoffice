using System;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.Products
{
    public class ProductCategory : Entity
    {

        public Guid ProductId { get; protected set; }

        public Guid CategoryId { get; protected set; }

        private ProductCategory()
        {

        }

        public ProductCategory(Guid productId, Guid categoryId)
        {
            ProductId = productId;
            CategoryId = categoryId;
        }

        public override object[] GetKeys()
        {
            return new object[]
                {
                    ProductId,
                    CategoryId
                };
        }
    }
}