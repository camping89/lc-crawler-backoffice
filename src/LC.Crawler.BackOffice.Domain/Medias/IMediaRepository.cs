using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.Medias
{
    public interface IMediaRepository : IRepository<Media, Guid>
    {
        Task<List<Media>> GetListAsync(
            string filterText = null,
            string name = null,
            string contentType = null,
            string url = null,
            string description = null,
            bool? isDowloaded = null,
            string sorting = null,
            int maxResultCount = int.MaxValue,
            int skipCount = 0,
            CancellationToken cancellationToken = default
        );

        Task<long> GetCountAsync(
            string filterText = null,
            string name = null,
            string contentType = null,
            string url = null,
            string description = null,
            bool? isDowloaded = null,
            CancellationToken cancellationToken = default);
    }
}