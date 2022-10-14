using System;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ProductVariants;

public interface IProductVariantAladinRepository : IRepository<ProductVariant, Guid>
{
}