using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.Products
{
    public interface IProductLongChauRepository : IProductRepository
    {
        
    }
    public interface IProductRepository : IRepository<Product, Guid>
    {
        Task<ProductWithNavigationProperties> GetWithNavigationPropertiesAsync(
    Guid id,
    CancellationToken cancellationToken = default
);

        Task<List<ProductWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string name = null,
            string brand = null,
            double? ratingMin = null,
            double? ratingMax = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            double? discountPercentMin = null,
            double? discountPercentMax = null,
            string shortDescription = null,
            string description = null,
            Guid? categoryId = null,
            Guid? mediaId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default
        );

        Task<List<Product>> GetListAsync(
                    string filterText = null,
                    string name = null,
                    string brand = null,
                    double? ratingMin = null,
                    double? ratingMax = null,
                    decimal? priceMin = null,
                    decimal? priceMax = null,
                    double? discountPercentMin = null,
                    double? discountPercentMax = null,
                    string shortDescription = null,
                    string description = null,
                    string sorting = null,
                    int maxResultCount = int.MaxValue,
                    int skipCount = 0,
                    CancellationToken cancellationToken = default
                );

        Task<long> GetCountAsync(
            string filterText = null,
            string name = null,
            string brand = null,
            double? ratingMin = null,
            double? ratingMax = null,
            decimal? priceMin = null,
            decimal? priceMax = null,
            double? discountPercentMin = null,
            double? discountPercentMax = null,
            string shortDescription = null,
            string description = null,
            Guid? categoryId = null,
            Guid? mediaId = null,
            CancellationToken cancellationToken = default);
    }
}