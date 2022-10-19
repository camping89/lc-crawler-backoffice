using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Categories;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using LC.Crawler.BackOffice.Permissions;
using LC.Crawler.BackOffice.Categories;

namespace LC.Crawler.BackOffice.Categories
{

    [Authorize(BackOfficePermissions.Categories.Default)]
    public class CategoriesAppService : ApplicationService, ICategoriesAppService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly CategoryManager _categoryManager;

        public CategoriesAppService(ICategoryRepository categoryRepository, CategoryManager categoryManager)
        {
            _categoryRepository = categoryRepository;
            _categoryManager = categoryManager;
        }

        public virtual async Task<PagedResultDto<CategoryWithNavigationPropertiesDto>> GetListAsync(GetCategoriesInput input)
        {
            var totalCount = await _categoryRepository.GetCountAsync(input.FilterText, input.Name, input.Slug, input.Description, input.CategoryType, input.ParentCategoryId);
            var items = await _categoryRepository.GetListWithNavigationPropertiesAsync(input.FilterText, input.Name, input.Slug, input.Description, input.CategoryType, input.ParentCategoryId, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<CategoryWithNavigationPropertiesDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<CategoryWithNavigationProperties>, List<CategoryWithNavigationPropertiesDto>>(items)
            };
        }

        public virtual async Task<CategoryWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id)
        {
            return ObjectMapper.Map<CategoryWithNavigationProperties, CategoryWithNavigationPropertiesDto>
                (await _categoryRepository.GetWithNavigationPropertiesAsync(id));
        }

        public virtual async Task<CategoryDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<Category, CategoryDto>(await _categoryRepository.GetAsync(id));
        }

        public virtual async Task<PagedResultDto<LookupDto<Guid?>>> GetCategoryLookupAsync(LookupRequestDto input)
        {
            var query = (await _categoryRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    x => x.Name != null &&
                         x.Name.Contains(input.Filter));

            var lookupData = await query.PageBy(input.SkipCount, input.MaxResultCount).ToDynamicListAsync<Category>();
            var totalCount = query.Count();
            return new PagedResultDto<LookupDto<Guid?>>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Category>, List<LookupDto<Guid?>>>(lookupData)
            };
        }

        [Authorize(BackOfficePermissions.Categories.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _categoryRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.Categories.Create)]
        public virtual async Task<CategoryDto> CreateAsync(CategoryCreateDto input)
        {

            var category = await _categoryManager.CreateAsync(
            input.ParentCategoryId, input.Name, input.Slug, input.Description, input.CategoryType
            );

            return ObjectMapper.Map<Category, CategoryDto>(category);
        }

        [Authorize(BackOfficePermissions.Categories.Edit)]
        public virtual async Task<CategoryDto> UpdateAsync(Guid id, CategoryUpdateDto input)
        {

            var category = await _categoryManager.UpdateAsync(
            id,
            input.ParentCategoryId, input.Name, input.Slug, input.Description, input.CategoryType, input.ConcurrencyStamp
            );

            return ObjectMapper.Map<Category, CategoryDto>(category);
        }
    }
}