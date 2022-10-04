using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.Shared;

namespace LC.Crawler.BackOffice.Web.Pages.ProductComments
{
    public class IndexModel : AbpPageModel
    {
        public string NameFilter { get; set; }
        public string ContentFilter { get; set; }
        public int? LikesFilterMin { get; set; }

        public int? LikesFilterMax { get; set; }
        public DateTime? CreatedAtFilterMin { get; set; }

        public DateTime? CreatedAtFilterMax { get; set; }

        private readonly IProductCommentsAppService _productCommentsAppService;

        public IndexModel(IProductCommentsAppService productCommentsAppService)
        {
            _productCommentsAppService = productCommentsAppService;
        }

        public async Task OnGetAsync()
        {

            await Task.CompletedTask;
        }
    }
}