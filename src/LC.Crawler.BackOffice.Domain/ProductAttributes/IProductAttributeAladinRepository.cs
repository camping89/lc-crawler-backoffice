using System;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ProductAttributes;

public interface IProductAttributeAladinRepository : IRepository<ProductAttribute, Guid>
{
}