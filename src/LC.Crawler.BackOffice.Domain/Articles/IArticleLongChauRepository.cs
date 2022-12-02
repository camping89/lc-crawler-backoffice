using System;
using System.Threading;
using System.Threading.Tasks;

namespace LC.Crawler.BackOffice.Articles;

public interface IArticleLongChauRepository : IArticleRepository
{
    Task DeleteOneById(Guid id, CancellationToken cancellationToken = default);
}