using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Articles;
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
using LC.Crawler.BackOffice.ArticleComments;

namespace LC.Crawler.BackOffice.ArticleComments
{

    [Authorize(BackOfficePermissions.ArticleComments.Default)]
    public class ArticleCommentsAppService : ApplicationService, IArticleCommentsAppService
    {
        private readonly IArticleCommentRepository _articleCommentRepository;
        private readonly ArticleCommentManager _articleCommentManager;
        private readonly IRepository<Article, Guid> _articleRepository;

        public ArticleCommentsAppService(IArticleCommentRepository articleCommentRepository, ArticleCommentManager articleCommentManager, IRepository<Article, Guid> articleRepository)
        {
            _articleCommentRepository = articleCommentRepository;
            _articleCommentManager = articleCommentManager; _articleRepository = articleRepository;
        }

        public virtual async Task<PagedResultDto<ArticleCommentWithNavigationPropertiesDto>> GetListAsync(GetArticleCommentsInput input)
        {
            var totalCount = await _articleCommentRepository.GetCountAsync(input.FilterText, input.Name, input.Content, input.LikesMin, input.LikesMax, input.CreatedAtMin, input.CreatedAtMax, input.ArticleId);
            var items = await _articleCommentRepository.GetListWithNavigationPropertiesAsync(input.FilterText, input.Name, input.Content, input.LikesMin, input.LikesMax, input.CreatedAtMin, input.CreatedAtMax, input.ArticleId, input.Sorting, input.MaxResultCount, input.SkipCount);

            return new PagedResultDto<ArticleCommentWithNavigationPropertiesDto>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<ArticleCommentWithNavigationProperties>, List<ArticleCommentWithNavigationPropertiesDto>>(items)
            };
        }

        public virtual async Task<ArticleCommentWithNavigationPropertiesDto> GetWithNavigationPropertiesAsync(Guid id)
        {
            return ObjectMapper.Map<ArticleCommentWithNavigationProperties, ArticleCommentWithNavigationPropertiesDto>
                (await _articleCommentRepository.GetWithNavigationPropertiesAsync(id));
        }

        public virtual async Task<ArticleCommentDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<ArticleComment, ArticleCommentDto>(await _articleCommentRepository.GetAsync(id));
        }

        public virtual async Task<PagedResultDto<LookupDto<Guid>>> GetArticleLookupAsync(LookupRequestDto input)
        {
            var query = (await _articleRepository.GetQueryableAsync())
                .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                    x => x.Title != null &&
                         x.Title.Contains(input.Filter));

            var lookupData = await query.PageBy(input.SkipCount, input.MaxResultCount).ToDynamicListAsync<Article>();
            var totalCount = query.Count();
            return new PagedResultDto<LookupDto<Guid>>
            {
                TotalCount = totalCount,
                Items = ObjectMapper.Map<List<Article>, List<LookupDto<Guid>>>(lookupData)
            };
        }

        [Authorize(BackOfficePermissions.ArticleComments.Delete)]
        public virtual async Task DeleteAsync(Guid id)
        {
            await _articleCommentRepository.DeleteAsync(id);
        }

        [Authorize(BackOfficePermissions.ArticleComments.Create)]
        public virtual async Task<ArticleCommentDto> CreateAsync(ArticleCommentCreateDto input)
        {
            if (input.ArticleId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["Article"]]);
            }

            var articleComment = await _articleCommentManager.CreateAsync(
            input.ArticleId, input.Name, input.Content, input.Likes, input.CreatedAt
            );

            return ObjectMapper.Map<ArticleComment, ArticleCommentDto>(articleComment);
        }

        [Authorize(BackOfficePermissions.ArticleComments.Edit)]
        public virtual async Task<ArticleCommentDto> UpdateAsync(Guid id, ArticleCommentUpdateDto input)
        {
            if (input.ArticleId == default)
            {
                throw new UserFriendlyException(L["The {0} field is required.", L["Article"]]);
            }

            var articleComment = await _articleCommentManager.UpdateAsync(
            id,
            input.ArticleId, input.Name, input.Content, input.Likes, input.CreatedAt
            );

            return ObjectMapper.Map<ArticleComment, ArticleCommentDto>(articleComment);
        }
    }
}