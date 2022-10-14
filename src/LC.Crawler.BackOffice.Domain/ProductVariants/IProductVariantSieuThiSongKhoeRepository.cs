using System;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ProductVariants;

public interface IProductVariantSieuThiSongKhoeRepository : IRepository<ProductVariant, Guid>
{
    
}