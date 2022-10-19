using LC.Crawler.BackOffice.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.Categories
{
    public interface ICategoryRepository : IRepository<Category, Guid>
    {
        Task<CategoryWithNavigationProperties> GetWithNavigationPropertiesAsync(
    Guid id,
    CancellationToken cancellationToken = default
);

        Task<List<CategoryWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string name = null,
            string slug = null,
            string description = null,
            CategoryType? categoryType = null,
            Guid? parentCategoryId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default
        );

        Task<List<Category>> GetListAsync(
                    string filterText = null,
                    string name = null,
                    string slug = null,
                    string description = null,
                    CategoryType? categoryType = null,
                    string sorting = null,
                    int maxResultCount = int.MaxValue,
                    int skipCount = 0,
                    CancellationToken cancellationToken = default
                );

        Task<long> GetCountAsync(
            string filterText = null,
            string name = null,
            string slug = null,
            string description = null,
            CategoryType? categoryType = null,
            Guid? parentCategoryId = null,
            CancellationToken cancellationToken = default);
    }
}