using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Products;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using LC.Crawler.BackOffice.ProductReviews;

namespace LC.Crawler.BackOffice.Web.Pages.ProductReviews
{
    public class EditModalModel : BackOfficePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public ProductReviewUpdateDto ProductReview { get; set; }

        public ProductDto Product { get; set; }

        private readonly IProductReviewsAppService _productReviewsAppService;

        public EditModalModel(IProductReviewsAppService productReviewsAppService)
        {
            _productReviewsAppService = productReviewsAppService;
        }

        public async Task OnGetAsync()
        {
            var productReviewWithNavigationPropertiesDto = await _productReviewsAppService.GetWithNavigationPropertiesAsync(Id);
            ProductReview = ObjectMapper.Map<ProductReviewDto, ProductReviewUpdateDto>(productReviewWithNavigationPropertiesDto.ProductReview);

            Product = productReviewWithNavigationPropertiesDto.Product;

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            await _productReviewsAppService.UpdateAsync(Id, ProductReview);
            return NoContent();
        }
    }
}