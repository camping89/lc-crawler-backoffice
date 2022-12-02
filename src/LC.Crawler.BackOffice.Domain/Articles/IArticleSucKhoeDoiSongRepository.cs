using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LC.Crawler.BackOffice.Articles;

public interface IArticleSucKhoeDoiSongRepository : IArticleRepository
{
    Task DeleteOneById(Guid id, CancellationToken cancellationToken = default);
}