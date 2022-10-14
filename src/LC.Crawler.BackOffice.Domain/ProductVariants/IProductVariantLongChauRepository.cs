using System;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ProductVariants;

public interface IProductVariantLongChauRepository : IRepository<ProductVariant, Guid>
{
}