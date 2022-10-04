using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using LC.Crawler.BackOffice.ArticleComments;
using LC.Crawler.BackOffice.Shared;

namespace LC.Crawler.BackOffice.Web.Pages.ArticleComments
{
    public class IndexModel : AbpPageModel
    {
        public string NameFilter { get; set; }
        public string ContentFilter { get; set; }
        public int? LikesFilterMin { get; set; }

        public int? LikesFilterMax { get; set; }
        public DateTime? CreatedAtFilterMin { get; set; }

        public DateTime? CreatedAtFilterMax { get; set; }

        private readonly IArticleCommentsAppService _articleCommentsAppService;

        public IndexModel(IArticleCommentsAppService articleCommentsAppService)
        {
            _articleCommentsAppService = articleCommentsAppService;
        }

        public async Task OnGetAsync()
        {

            await Task.CompletedTask;
        }
    }
}