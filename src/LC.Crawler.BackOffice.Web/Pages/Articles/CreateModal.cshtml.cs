using LC.Crawler.BackOffice.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LC.Crawler.BackOffice.Articles;

namespace LC.Crawler.BackOffice.Web.Pages.Articles
{
    public class CreateModalModel : BackOfficePageModel
    {
        [BindProperty]
        public ArticleCreateDto Article { get; set; }

        [BindProperty]
        public List<Guid> SelectedCategoryIds { get; set; }
        [BindProperty]
        public List<Guid> SelectedMediaIds { get; set; }

        private readonly IArticlesAppService _articlesAppService;

        public CreateModalModel(IArticlesAppService articlesAppService)
        {
            _articlesAppService = articlesAppService;
        }

        public async Task OnGetAsync()
        {
            Article = new ArticleCreateDto();

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            Article.CategoryIds = SelectedCategoryIds;

            Article.MediaIds = SelectedMediaIds;

            await _articlesAppService.CreateAsync(Article);
            return NoContent();
        }
    }
}