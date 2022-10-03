using System;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.Products
{
    public class ProductMedia : Entity
    {

        public Guid ProductId { get; protected set; }

        public Guid MediaId { get; protected set; }

        private ProductMedia()
        {

        }

        public ProductMedia(Guid productId, Guid mediaId)
        {
            ProductId = productId;
            MediaId = mediaId;
        }

        public override object[] GetKeys()
        {
            return new object[]
                {
                    ProductId,
                    MediaId
                };
        }
    }
}