using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ProductReviews
{
    public interface IProductReviewRepository : IRepository<ProductReview, Guid>
    {
        Task<ProductReviewWithNavigationProperties> GetWithNavigationPropertiesAsync(
    Guid id,
    CancellationToken cancellationToken = default
);

        Task<List<ProductReviewWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string name = null,
            string content = null,
            decimal? ratingMin = null,
            decimal? ratingMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            int? likesMin = null,
            int? likesMax = null,
            Guid? productId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default
        );

        Task<List<ProductReview>> GetListAsync(
                    string filterText = null,
                    string name = null,
                    string content = null,
                    decimal? ratingMin = null,
                    decimal? ratingMax = null,
                    DateTime? createdAtMin = null,
                    DateTime? createdAtMax = null,
                    int? likesMin = null,
                    int? likesMax = null,
                    string sorting = null,
                    int maxResultCount = int.MaxValue,
                    int skipCount = 0,
                    CancellationToken cancellationToken = default
                );

        Task<long> GetCountAsync(
            string filterText = null,
            string name = null,
            string content = null,
            decimal? ratingMin = null,
            decimal? ratingMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            int? likesMin = null,
            int? likesMax = null,
            Guid? productId = null,
            CancellationToken cancellationToken = default);
    }
}