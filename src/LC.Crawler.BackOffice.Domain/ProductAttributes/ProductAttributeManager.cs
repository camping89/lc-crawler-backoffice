using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.ProductAttributes
{
    public class ProductAttributeManager : DomainService
    {
        private readonly IProductAttributeRepository _productAttributeRepository;

        public ProductAttributeManager(IProductAttributeRepository productAttributeRepository)
        {
            _productAttributeRepository = productAttributeRepository;
        }

        public async Task<ProductAttribute> CreateAsync(
        Guid productId, string slug, string key, string value)
        {
            var productAttribute = new ProductAttribute(
             GuidGenerator.Create(),
             productId, slug, key, value
             );

            return await _productAttributeRepository.InsertAsync(productAttribute);
        }

        public async Task<ProductAttribute> UpdateAsync(
            Guid id,
            Guid productId, string slug, string key, string value
        )
        {
            var queryable = await _productAttributeRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var productAttribute = await AsyncExecuter.FirstOrDefaultAsync(query);

            productAttribute.ProductId = productId;
            productAttribute.Slug = slug;
            productAttribute.Key = key;
            productAttribute.Value = value;

            return await _productAttributeRepository.UpdateAsync(productAttribute);
        }

    }
}