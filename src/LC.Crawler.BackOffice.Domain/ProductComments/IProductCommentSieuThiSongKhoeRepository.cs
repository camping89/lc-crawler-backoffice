using System;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ProductComments;

public interface IProductCommentSieuThiSongKhoeRepository : IRepository<ProductComment, Guid>
{
}