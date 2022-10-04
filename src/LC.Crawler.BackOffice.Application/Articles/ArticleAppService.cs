using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
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
using LC.Crawler.BackOffice.Articles;

namespace LC.Crawler.BackOffice.Articles
{

    [Authorize(BackOfficePermissions.Articles.Default)]
    public class ArticlesAppService : ApplicationService, IArticlesAppService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly ArticleManager _articleManager;
        private readonly IRepository<Media, Guid> _mediaRepository;
        private readonly IRepository<DataSource, Guid> _dataSourceRepository;
        private readonly IRepository<Category, Guid> _categoryRepository;
        

        public ArticlesAppService(IArticleRepository articleRepository, ArticleManager articleManager, IRepository<Media, Guid> mediaRepository, IRepository<DataSource, Guid> dataSourceRepository, IRepository<Category, Guid> categoryRepository
        )
        {
            _articleRepository = articleRepository;
            _articleManager = articleManager;
            _mediaRepository = mediaRepository;
            _dataSourceRepository = dataSourceRepository;
            _categoryRepository = categoryRepository;
            
        }

        public virtual async Task<PagedResultDto<ArticleWithNavigationPropertiesDto>> GetListAsync(GetArticlesInput input)
        {
            var totalCount = await _articleRepository.GetCountAsync(input.FilterText, input.Title, input.Excerpt, input.Content, input.CreatedAtMin, input.CreatedAtMax, input.Author, input.Tags, input.LikeCountMin, input.LikeCountMax, input.CommentCountMin, input.CommentCountMax, input.ShareCountMin, input.ShareCountMax, input.FeaturedMediaId, input.DataSourceId, input.CategoryId, input.MediaId);
            var items = await _articleRepository.GetListWithNavigationPropertiesAsync(input.FilterText, input.Title, input.Excerpt, input.Content, input.CreatedAtMin, input.CreatedAtMax, input.Author, input.Tags, input.LikeCountMin, input.LikeCountMax, input.CommentCountMin, input.CommentCountMax, input.ShareCountMin, input.ShareCountMax, input.FeaturedMediaId, input.DataSourceId, input.CategoryId, input.MediaId, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<ArticleWithNavigationPropertiesDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<ArticleWithNavigationProperties>, List<ArticleWithNavigationPropertiesDto>>(items)
            };
        }

        public virtual async Task<ArticleWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id)
        {
            return ObjectMapper.Map<ArticleWithNavigationProperties, ArticleWithNavigationPropertiesDto>
                (await _articleRepository.GetWithNavigationPropertiesAsync(id));
        }

        public virtual async Task<ArticleDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<Article, ArticleDto>(await _articleRepository.GetAsync(id));
        }

        public virtual async Task<PagedResultDto<LookupDto<Guid?>>> GetMediaLookupAsync(LookupRequestDto input)
        {
            var query = (await _mediaRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    x => x.Url != null &&
                         x.Url.Contains(input.Filter));

            var lookupData = await query.PageBy(input.SkipCount, input.MaxResultCount).ToDynamicListAsync<Media>();
            var totalCount = query.Count();
            return new PagedResultDto<LookupDto<Guid?>>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Media>, List<LookupDto<Guid?>>>(lookupData)
            };
        }

        public virtual async Task<PagedResultDto<LookupDto<Guid>>> GetDataSourceLookupAsync(LookupRequestDto input)
        {
            var query = (await _dataSourceRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    x => x.Url != null &&
                         x.Url.Contains(input.Filter));

            var lookupData = await query.PageBy(input.SkipCount, input.MaxResultCount).ToDynamicListAsync<DataSource>();
            var totalCount = query.Count();
            return new PagedResultDto<LookupDto<Guid>>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<DataSource>, List<LookupDto<Guid>>>(lookupData)
            };
        }

        public virtual async Task<PagedResultDto<LookupDto<Guid>>> GetCategoryLookupAsync(LookupRequestDto input)
        {
            var query = (await _categoryRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    x => x.Name != null &&
                         x.Name.Contains(input.Filter));

            var lookupData = await query.PageBy(input.SkipCount, input.MaxResultCount).ToDynamicListAsync<Category>();
            var totalCount = query.Count();
            return new PagedResultDto<LookupDto<Guid>>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Category>, List<LookupDto<Guid>>>(lookupData)
            };
        }

        [Authorize(BackOfficePermissions.Articles.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _articleRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.Articles.Create)]
        public virtual async Task<ArticleDto> CreateAsync(ArticleCreateDto input)
        {
            if (input.DataSourceId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["DataSource"]]);
            }

            var article = await _articleManager.CreateAsync(
            input.CategoryIds, input.MediaIds, input.FeaturedMediaId, input.DataSourceId, input.Title, input.Excerpt, input.Content, input.CreatedAt, input.Author, input.Tags, input.LikeCount, input.CommentCount, input.ShareCount
            );

            return ObjectMapper.Map<Article, ArticleDto>(article);
        }

        [Authorize(BackOfficePermissions.Articles.Edit)]
        public virtual async Task<ArticleDto> UpdateAsync(Guid id, ArticleUpdateDto input)
        {
            if (input.DataSourceId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["DataSource"]]);
            }

            var article = await _articleManager.UpdateAsync(
            id,
            input.CategoryIds, input.MediaIds, input.FeaturedMediaId, input.DataSourceId, input.Title, input.Excerpt, input.Content, input.CreatedAt, input.Author, input.Tags, input.LikeCount, input.CommentCount, input.ShareCount, input.ConcurrencyStamp
            );

            return ObjectMapper.Map<Article, ArticleDto>(article);
        }
    }
}