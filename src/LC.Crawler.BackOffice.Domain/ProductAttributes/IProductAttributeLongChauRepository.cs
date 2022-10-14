using System;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ProductAttributes;

public interface IProductAttributeLongChauRepository : IRepository<ProductAttribute, Guid>
{
}