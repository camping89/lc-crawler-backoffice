using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ArticleComments
{
    public interface IArticleCommentRepository : IRepository<ArticleComment, Guid>
    {
        Task<ArticleCommentWithNavigationProperties> GetWithNavigationPropertiesAsync(
    Guid id,
    CancellationToken cancellationToken = default
);

        Task<List<ArticleCommentWithNavigationProperties>> GetListWithNavigationPropertiesAsync(
            string filterText = null,
            string name = null,
            string content = null,
            int? likesMin = null,
            int? likesMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            Guid? articleId = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default
        );

        Task<List<ArticleComment>> GetListAsync(
                    string filterText = null,
                    string name = null,
                    string content = null,
                    int? likesMin = null,
                    int? likesMax = null,
                    DateTime? createdAtMin = null,
                    DateTime? createdAtMax = null,
                    string sorting = null,
                    int maxResultCount = int.MaxValue,
                    int skipCount = 0,
                    CancellationToken cancellationToken = default
                );

        Task<long> GetCountAsync(
            string filterText = null,
            string name = null,
            string content = null,
            int? likesMin = null,
            int? likesMax = null,
            DateTime? createdAtMin = null,
            DateTime? createdAtMax = null,
            Guid? articleId = null,
            CancellationToken cancellationToken = default);
    }
}