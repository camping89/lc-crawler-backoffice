using LC.Crawler.BackOffice.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LC.Crawler.BackOffice.ProductReviews;

namespace LC.Crawler.BackOffice.Web.Pages.ProductReviews
{
    public class CreateModalModel : BackOfficePageModel
    {
        [BindProperty]
        public ProductReviewCreateDto ProductReview { get; set; }

        private readonly IProductReviewsAppService _productReviewsAppService;

        public CreateModalModel(IProductReviewsAppService productReviewsAppService)
        {
            _productReviewsAppService = productReviewsAppService;
        }

        public async Task OnGetAsync()
        {
            ProductReview = new ProductReviewCreateDto();

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            await _productReviewsAppService.CreateAsync(ProductReview);
            return NoContent();
        }
    }
}