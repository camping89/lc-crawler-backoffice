using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.Shared;

namespace LC.Crawler.BackOffice.Web.Pages.ProductReviews
{
    public class IndexModel : AbpPageModel
    {
        public string NameFilter { get; set; }
        public string ContentFilter { get; set; }
        public decimal? RatingFilterMin { get; set; }

        public decimal? RatingFilterMax { get; set; }
        public DateTime? CreatedAtFilterMin { get; set; }

        public DateTime? CreatedAtFilterMax { get; set; }
        public int? LikesFilterMin { get; set; }

        public int? LikesFilterMax { get; set; }

        private readonly IProductReviewsAppService _productReviewsAppService;

        public IndexModel(IProductReviewsAppService productReviewsAppService)
        {
            _productReviewsAppService = productReviewsAppService;
        }

        public async Task OnGetAsync()
        {

            await Task.CompletedTask;
        }
    }
}