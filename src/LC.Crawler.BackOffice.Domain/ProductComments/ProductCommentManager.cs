using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.ProductComments
{
    public class ProductCommentManager : DomainService
    {
        private readonly IProductCommentRepository _productCommentRepository;

        public ProductCommentManager(IProductCommentRepository productCommentRepository)
        {
            _productCommentRepository = productCommentRepository;
        }

        public async Task<ProductComment> CreateAsync(
        Guid productId, string name, string content, int likes, DateTime? createdAt = null)
        {
            var productComment = new ProductComment(
             GuidGenerator.Create(),
             productId, name, content, likes, createdAt
             );

            return await _productCommentRepository.InsertAsync(productComment);
        }

        public async Task<ProductComment> UpdateAsync(
            Guid id,
            Guid productId, string name, string content, int likes, DateTime? createdAt = null
        )
        {
            var queryable = await _productCommentRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var productComment = await AsyncExecuter.FirstOrDefaultAsync(query);

            productComment.ProductId = productId;
            productComment.Name = name;
            productComment.Content = content;
            productComment.Likes = likes;
            productComment.CreatedAt = createdAt;

            return await _productCommentRepository.UpdateAsync(productComment);
        }

    }
}