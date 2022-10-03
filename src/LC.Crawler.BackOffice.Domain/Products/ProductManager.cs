using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Medias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Data;

namespace LC.Crawler.BackOffice.Products
{
    public class ProductManager : DomainService
    {
        private readonly IProductRepository _productRepository;
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IRepository<Media, Guid> _mediaRepository;

        public ProductManager(IProductRepository productRepository,
        IRepository<Category, Guid> categoryRepository,
        IRepository<Media, Guid> mediaRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mediaRepository = mediaRepository;
        }

        public async Task<Product> CreateAsync(
        List<Guid> categoryIds,
        List<Guid> mediaIds,
        string name, string brand, double rating, decimal price, double discountPercent, string shortDescription, string description)
        {
            var product = new Product(
             GuidGenerator.Create(),
             name, brand, rating, price, discountPercent, shortDescription, description
             );

            await SetCategoriesAsync(product, categoryIds);
            await SetMediasAsync(product, mediaIds);

            return await _productRepository.InsertAsync(product);
        }

        public async Task<Product> UpdateAsync(
            Guid id,
            List<Guid> categoryIds,
        List<Guid> mediaIds,
        string name, string brand, double rating, decimal price, double discountPercent, string shortDescription, string description, [CanBeNull] string concurrencyStamp = null
        )
        {
            var queryable = await _productRepository.WithDetailsAsync(x => x.Categories, x => x.Medias);
            var query = queryable.Where(x => x.Id == id);

            var product = await AsyncExecuter.FirstOrDefaultAsync(query);

            product.Name = name;
            product.Brand = brand;
            product.Rating = rating;
            product.Price = price;
            product.DiscountPercent = discountPercent;
            product.ShortDescription = shortDescription;
            product.Description = description;

            await SetCategoriesAsync(product, categoryIds);
            await SetMediasAsync(product, mediaIds);

            product.SetConcurrencyStampIfNotNull(concurrencyStamp);
            return await _productRepository.UpdateAsync(product);
        }

        private async Task SetCategoriesAsync(Product product, List<Guid> categoryIds)
        {
            if (categoryIds == null || !categoryIds.Any())
            {
                product.RemoveAllCategories();
                return;
            }

            var query = (await _categoryRepository.GetQueryableAsync())
                .Where(x => categoryIds.Contains(x.Id))
                .Select(x => x.Id);

            var categoryIdsInDb = await AsyncExecuter.ToListAsync(query);
            if (!categoryIdsInDb.Any())
            {
                return;
            }

            product.RemoveAllCategoriesExceptGivenIds(categoryIdsInDb);

            foreach (var categoryId in categoryIdsInDb)
            {
                product.AddCategory(categoryId);
            }
        }

        private async Task SetMediasAsync(Product product, List<Guid> mediaIds)
        {
            if (mediaIds == null || !mediaIds.Any())
            {
                product.RemoveAllMedias();
                return;
            }

            var query = (await _mediaRepository.GetQueryableAsync())
                .Where(x => mediaIds.Contains(x.Id))
                .Select(x => x.Id);

            var mediaIdsInDb = await AsyncExecuter.ToListAsync(query);
            if (!mediaIdsInDb.Any())
            {
                return;
            }

            product.RemoveAllMediasExceptGivenIds(mediaIdsInDb);

            foreach (var mediaId in mediaIdsInDb)
            {
                product.AddMedia(mediaId);
            }
        }

    }
}