using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ProductVariants
{
    public interface IProductVariantRepository : IRepository<ProductVariant, Guid>
    {
        Task<ProductVariantWithNavigationProperties> GetWithNavigationPropertiesAsync(
    Guid id,
    CancellationToken cancellationToken = default
);

        Task<List<ProductVariantWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string sKU = null,
            decimal? retailPriceMin = null,
            decimal? retailPriceMax = null,
            double? discountRateMin = null,
            double? discountRateMax = null,
            decimal? discountedPriceMin = null,
            decimal? discountedPriceMax = null,
            Guid? productId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default
        );

        Task<List<ProductVariant>> GetListAsync(
                    string filterText = null,
                    string sKU = null,
                    decimal? retailPriceMin = null,
                    decimal? retailPriceMax = null,
                    double? discountRateMin = null,
                    double? discountRateMax = null,
                    decimal? discountedPriceMin = null,
                    decimal? discountedPriceMax = null,
                    string sorting = null,
                    int maxResultCount = int.MaxValue,
                    int skipCount = 0,
                    CancellationToken cancellationToken = default
                );

        Task<long> GetCountAsync(
            string filterText = null,
            string sKU = null,
            decimal? retailPriceMin = null,
            decimal? retailPriceMax = null,
            double? discountRateMin = null,
            double? discountRateMax = null,
            decimal? discountedPriceMin = null,
            decimal? discountedPriceMax = null,
            Guid? productId = null,
            CancellationToken cancellationToken = default);
    }
}