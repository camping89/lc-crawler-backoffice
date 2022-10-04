using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using LC.Crawler.BackOffice.Articles;

namespace LC.Crawler.BackOffice.Web.Pages.Articles
{
    public class EditModalModel : BackOfficePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public ArticleUpdateDto Article { get; set; }

        public MediaDto Media { get; set; }
        public DataSourceDto DataSource { get; set; }
        public List<CategoryDto> Categories { get; set; }
        public List<MediaDto> Medias { get; set; }
        [BindProperty]
        public List<Guid> SelectedCategoryIds { get; set; }
        [BindProperty]
        public List<Guid> SelectedMediaIds { get; set; }

        private readonly IArticlesAppService _articlesAppService;

        public EditModalModel(IArticlesAppService articlesAppService)
        {
            _articlesAppService = articlesAppService;
        }

        public async Task OnGetAsync()
        {
            var articleWithNavigationPropertiesDto = await _articlesAppService.GetWithNavigationPropertiesAsync(Id);
            Article = ObjectMapper.Map<ArticleDto, ArticleUpdateDto>(articleWithNavigationPropertiesDto.Article);

            Media = articleWithNavigationPropertiesDto.Media;
            DataSource = articleWithNavigationPropertiesDto.DataSource;
            Categories = articleWithNavigationPropertiesDto.Categories;
            Medias = articleWithNavigationPropertiesDto.Medias;

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            Article.CategoryIds = SelectedCategoryIds;

            Article.MediaIds = SelectedMediaIds;

            await _articlesAppService.UpdateAsync(Id, Article);
            return NoContent();
        }
    }
}