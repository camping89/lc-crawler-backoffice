using LC.Crawler.BackOffice.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LC.Crawler.BackOffice.ArticleComments;

namespace LC.Crawler.BackOffice.Web.Pages.ArticleComments
{
    public class CreateModalModel : BackOfficePageModel
    {
        [BindProperty]
        public ArticleCommentCreateDto ArticleComment { get; set; }

        private readonly IArticleCommentsAppService _articleCommentsAppService;

        public CreateModalModel(IArticleCommentsAppService articleCommentsAppService)
        {
            _articleCommentsAppService = articleCommentsAppService;
        }

        public async Task OnGetAsync()
        {
            ArticleComment = new ArticleCommentCreateDto();

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            await _articleCommentsAppService.CreateAsync(ArticleComment);
            return NoContent();
        }
    }
}