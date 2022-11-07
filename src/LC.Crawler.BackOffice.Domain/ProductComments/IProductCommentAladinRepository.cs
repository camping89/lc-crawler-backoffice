using System;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ProductComments;

public interface IProductCommentAladinRepository : IRepository<ProductComment, Guid>
{
}