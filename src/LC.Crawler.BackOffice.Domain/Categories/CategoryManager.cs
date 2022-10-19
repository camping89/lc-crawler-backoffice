using LC.Crawler.BackOffice.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Data;

namespace LC.Crawler.BackOffice.Categories
{
    public class CategoryManager : DomainService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryManager(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Category> CreateAsync(
        Guid? parentCategoryId, string name, string slug, string description, CategoryType categoryType)
        {
            var category = new Category(
             GuidGenerator.Create(),
             parentCategoryId, name, slug, description, categoryType
             );

            return await _categoryRepository.InsertAsync(category);
        }

        public async Task<Category> UpdateAsync(
            Guid id,
            Guid? parentCategoryId, string name, string slug, string description, CategoryType categoryType, [CanBeNull] string concurrencyStamp = null
        )
        {
            var queryable = await _categoryRepository.GetQueryableAsync();
            var query = queryable.Where(x => x.Id == id);

            var category = await AsyncExecuter.FirstOrDefaultAsync(query);

            category.ParentCategoryId = parentCategoryId;
            category.Name = name;
            category.Slug = slug;
            category.Description = description;
            category.CategoryType = categoryType;

            category.SetConcurrencyStampIfNotNull(concurrencyStamp);
            return await _categoryRepository.UpdateAsync(category);
        }

    }
}