using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Categories;
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

        public List<CategoryDto> Categories { get; set; }
        [BindProperty]
        public List<Guid> SelectedCategoryIds { get; set; }

        private readonly IArticlesAppService _articlesAppService;

        public EditModalModel(IArticlesAppService articlesAppService)
        {
            _articlesAppService = articlesAppService;
        }

        public async Task OnGetAsync()
        {
            var articleWithNavigationPropertiesDto = await _articlesAppService.GetWithNavigationPropertiesAsync(Id);
            Article = ObjectMapper.Map<ArticleDto, ArticleUpdateDto>(articleWithNavigationPropertiesDto.Article);

            Categories = articleWithNavigationPropertiesDto.Categories;

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            Article.CategoryIds = SelectedCategoryIds;

            await _articlesAppService.UpdateAsync(Id, Article);
            return NoContent();
        }
    }
}