using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.ArticleComments;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace LC.Crawler.BackOffice.Publics;
[RemoteService(IsEnabled = false)]
public class ArticlePublicAppService : ApplicationService,IArticlePublicAppService
{
     private readonly IArticleRepository _articleRepository;
        private readonly ArticleManager _articleManager;
        private readonly IRepository<Media, Guid> _mediaRepository;
        private readonly IRepository<DataSource, Guid> _dataSourceRepository;
        private readonly IRepository<Category, Guid> _categoryRepository;
        private readonly IArticleCommentRepository _articleCommentRepository;


        public ArticlePublicAppService(IArticleRepository articleRepository, ArticleManager articleManager, IRepository<Media, Guid> mediaRepository, IRepository<DataSource, Guid> dataSourceRepository, IRepository<Category, Guid> categoryRepository,
            IArticleCommentRepository articleCommentRepository)
        {
            _articleRepository = articleRepository;
            _articleManager = articleManager;
            _mediaRepository = mediaRepository;
            _dataSourceRepository = dataSourceRepository;
            _categoryRepository = categoryRepository;
            _articleCommentRepository = articleCommentRepository;
        }

        public virtual async Task<PagedResultDto<ArticleWithNavigationPropertiesResultDto>> GetListAsync(GetArticlesInput input)
        {
            var totalCount = await _articleRepository.GetCountAsync(input.FilterText, input.Title, input.Excerpt, input.Content, input.CreatedAtMin, input.CreatedAtMax, input.Author, input.Tags, input.LikeCountMin, input.LikeCountMax, input.CommentCountMin, input.CommentCountMax, input.ShareCountMin, input.ShareCountMax, input.FeaturedMediaId, input.DataSourceId, input.CategoryId, input.MediaId);
            var items = await _articleRepository.GetListWithNavigationPropertiesAsync(input.FilterText, input.Title, input.Excerpt, input.Content, input.CreatedAtMin, input.CreatedAtMax, input.Author, input.Tags, input.LikeCountMin, input.LikeCountMax, input.CommentCountMin, input.CommentCountMax, input.ShareCountMin, input.ShareCountMax, input.FeaturedMediaId, input.DataSourceId, input.CategoryId, input.MediaId, input.Sorting, input.MaxResultCount, input.SkipCount);
            var results = new List<ArticleWithNavigationPropertiesResultDto>();
            foreach (var item in items)
            {
                var resultItem = new ArticleWithNavigationPropertiesResultDto
                {
                    DataSource = item.DataSource.Url
                };

                ObjectMapper.Map(item.Article, resultItem.Article);
                ObjectMapper.Map(item.Categories, resultItem.Categories);
                ObjectMapper.Map(item.Medias, resultItem.Medias);
                if (item.Article is { Tags: { } })
                {
                    resultItem.Article.Tags = item.Article.Tags;
                }
            }
            return new PagedResultDto<ArticleWithNavigationPropertiesResultDto>
            {
                TotalCount = totalCount,
                Items = results
            };
        }

        public async Task<ArticleCommentsResultDto> GetArticleCommentsAsync(Guid articleId)
        {
            var comments = await _articleCommentRepository.GetListAsync(x => x.ArticleId == articleId);
            var articleNav = await _articleRepository.GetWithNavigationPropertiesAsync(articleId);
            return new ArticleCommentsResultDto()
            {
                DataSource = articleNav.DataSource.Url,
                Article = ObjectMapper.Map<Article,ArticleResultDto>(articleNav.Article),
                Comments = ObjectMapper.Map<List<ArticleComment>, List<ArticleCommentResultDto>>(comments)
            };
        }
}