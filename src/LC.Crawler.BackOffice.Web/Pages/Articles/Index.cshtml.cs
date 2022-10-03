using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Shared;

namespace LC.Crawler.BackOffice.Web.Pages.Articles
{
    public class IndexModel : AbpPageModel
    {
        public string TitleFilter { get; set; }
        public string ExcerptFilter { get; set; }
        public string ContentFilter { get; set; }
        public DateTime? CreatedAtFilterMin { get; set; }

        public DateTime? CreatedAtFilterMax { get; set; }
        public string AuthorFilter { get; set; }
        public string TagsFilter { get; set; }
        public int? LikeCountFilterMin { get; set; }

        public int? LikeCountFilterMax { get; set; }
        public int? CommentCountFilterMin { get; set; }

        public int? CommentCountFilterMax { get; set; }
        public int? ShareCountFilterMin { get; set; }

        public int? ShareCountFilterMax { get; set; }

        private readonly IArticlesAppService _articlesAppService;

        public IndexModel(IArticlesAppService articlesAppService)
        {
            _articlesAppService = articlesAppService;
        }

        public async Task OnGetAsync()
        {

            await Task.CompletedTask;
        }
    }
}