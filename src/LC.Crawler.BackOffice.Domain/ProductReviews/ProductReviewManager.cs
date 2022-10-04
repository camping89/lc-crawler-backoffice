using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.ProductReviews
{
    public class ProductReviewManager : DomainService
    {
        private readonly IProductReviewRepository _productReviewRepository;

        public ProductReviewManager(IProductReviewRepository productReviewRepository)
        {
            _productReviewRepository = productReviewRepository;
        }

        public async Task<ProductReview> CreateAsync(
        Guid productId, string name, string content, decimal rating, int likes, DateTime? createdAt = null)
        {
            var productReview = new ProductReview(
             GuidGenerator.Create(),
             productId, name, content, rating, likes, createdAt
             );

            return await _productReviewRepository.InsertAsync(productReview);
        }

        public async Task<ProductReview> UpdateAsync(
            Guid id,
            Guid productId, string name, string content, decimal rating, int likes, DateTime? createdAt = null
        )
        {
            var queryable = await _productReviewRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var productReview = await AsyncExecuter.FirstOrDefaultAsync(query);

            productReview.ProductId = productId;
            productReview.Name = name;
            productReview.Content = content;
            productReview.Rating = rating;
            productReview.Likes = likes;
            productReview.CreatedAt = createdAt;

            return await _productReviewRepository.UpdateAsync(productReview);
        }

    }
}