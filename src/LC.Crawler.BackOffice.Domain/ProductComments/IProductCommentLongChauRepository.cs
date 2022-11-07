using System;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ProductComments;

public interface IProductCommentLongChauRepository : IRepository<ProductComment, Guid>
{
}