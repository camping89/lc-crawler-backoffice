using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Articles;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using LC.Crawler.BackOffice.ArticleComments;

namespace LC.Crawler.BackOffice.Web.Pages.ArticleComments
{
    public class EditModalModel : BackOfficePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public ArticleCommentUpdateDto ArticleComment { get; set; }

        public ArticleDto Article { get; set; }

        private readonly IArticleCommentsAppService _articleCommentsAppService;

        public EditModalModel(IArticleCommentsAppService articleCommentsAppService)
        {
            _articleCommentsAppService = articleCommentsAppService;
        }

        public async Task OnGetAsync()
        {
            var articleCommentWithNavigationPropertiesDto = await _articleCommentsAppService.GetWithNavigationPropertiesAsync(Id);
            ArticleComment = ObjectMapper.Map<ArticleCommentDto, ArticleCommentUpdateDto>(articleCommentWithNavigationPropertiesDto.ArticleComment);

            Article = articleCommentWithNavigationPropertiesDto.Article;

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            await _articleCommentsAppService.UpdateAsync(Id, ArticleComment);
            return NoContent();
        }
    }
}