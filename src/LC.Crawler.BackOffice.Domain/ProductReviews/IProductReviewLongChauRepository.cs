using System;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.ProductReviews;

public interface IProductReviewLongChauRepository: IRepository<ProductReview, Guid>
{
    
}