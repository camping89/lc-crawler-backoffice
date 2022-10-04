using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.ProductVariants
{
    public class ProductVariantManager : DomainService
    {
        private readonly IProductVariantRepository _productVariantRepository;

        public ProductVariantManager(IProductVariantRepository productVariantRepository)
        {
            _productVariantRepository = productVariantRepository;
        }

        public async Task<ProductVariant> CreateAsync(
        Guid productId, string sKU, decimal retailPrice, double discountRate, decimal discountedPrice)
        {
            var productVariant = new ProductVariant(
             GuidGenerator.Create(),
             productId, sKU, retailPrice, discountRate, discountedPrice
             );

            return await _productVariantRepository.InsertAsync(productVariant);
        }

        public async Task<ProductVariant> UpdateAsync(
            Guid id,
            Guid productId, string sKU, decimal retailPrice, double discountRate, decimal discountedPrice
        )
        {
            var queryable = await _productVariantRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var productVariant = await AsyncExecuter.FirstOrDefaultAsync(query);

            productVariant.ProductId = productId;
            productVariant.SKU = sKU;
            productVariant.RetailPrice = retailPrice;
            productVariant.DiscountRate = discountRate;
            productVariant.DiscountedPrice = discountedPrice;

            return await _productVariantRepository.UpdateAsync(productVariant);
        }

    }
}