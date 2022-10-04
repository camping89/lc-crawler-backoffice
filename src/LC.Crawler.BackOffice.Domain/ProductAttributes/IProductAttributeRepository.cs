using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ProductAttributes
{
    public interface IProductAttributeRepository : IRepository<ProductAttribute, Guid>
    {
        Task<ProductAttributeWithNavigationProperties> GetWithNavigationPropertiesAsync(
    Guid id,
    CancellationToken cancellationToken = default
);

        Task<List<ProductAttributeWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string slug = null,
            string key = null,
            string value = null,
            Guid? productId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default
        );

        Task<List<ProductAttribute>> GetListAsync(
                    string filterText = null,
                    string slug = null,
                    string key = null,
                    string value = null,
                    string sorting = null,
                    int maxResultCount = int.MaxValue,
                    int skipCount = 0,
                    CancellationToken cancellationToken = default
                );

        Task<long> GetCountAsync(
            string filterText = null,
            string slug = null,
            string key = null,
            string value = null,
            Guid? productId = null,
            CancellationToken cancellationToken = default);
    }
}