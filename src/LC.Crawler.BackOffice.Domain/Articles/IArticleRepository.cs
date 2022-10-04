using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.Articles
{
    public interface IArticleRepository : IRepository<Article, Guid>
    {
        Task<ArticleWithNavigationProperties> GetWithNavigationPropertiesAsync(
    Guid id,
    CancellationToken cancellationToken = default
);

        Task<List<ArticleWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string title = null,
            string excerpt = null,
            string content = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            string author = null,
            string tags = null,
            int? likeCountMin = null,
            int? likeCountMax = null,
            int? commentCountMin = null,
            int? commentCountMax = null,
            int? shareCountMin = null,
            int? shareCountMax = null,
            Guid? featuredMediaId = null,
            Guid? dataSourceId = null,
            Guid? categoryId = null,
            Guid? mediaId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default
        );

        Task<List<Article>> GetListAsync(
                    string filterText = null,
                    string title = null,
                    string excerpt = null,
                    string content = null,
                    DateTime? createdAtMin = null,
                    DateTime? createdAtMax = null,
                    string author = null,
                    string tags = null,
                    int? likeCountMin = null,
                    int? likeCountMax = null,
                    int? commentCountMin = null,
                    int? commentCountMax = null,
                    int? shareCountMin = null,
                    int? shareCountMax = null,
                    string sorting = null,
                    int maxResultCount = int.MaxValue,
                    int skipCount = 0,
                    CancellationToken cancellationToken = default
                );

        Task<long> GetCountAsync(
            string filterText = null,
            string title = null,
            string excerpt = null,
            string content = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            string author = null,
            string tags = null,
            int? likeCountMin = null,
            int? likeCountMax = null,
            int? commentCountMin = null,
            int? commentCountMax = null,
            int? shareCountMin = null,
            int? shareCountMax = null,
            Guid? featuredMediaId = null,
            Guid? dataSourceId = null,
            Guid? categoryId = null,
            Guid? mediaId = null,
            CancellationToken cancellationToken = default);
    }
}