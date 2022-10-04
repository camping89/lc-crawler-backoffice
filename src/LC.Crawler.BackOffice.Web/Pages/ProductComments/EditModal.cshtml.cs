using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Products;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using LC.Crawler.BackOffice.ProductComments;

namespace LC.Crawler.BackOffice.Web.Pages.ProductComments
{
    public class EditModalModel : BackOfficePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public ProductCommentUpdateDto ProductComment { get; set; }

        public ProductDto Product { get; set; }

        private readonly IProductCommentsAppService _productCommentsAppService;

        public EditModalModel(IProductCommentsAppService productCommentsAppService)
        {
            _productCommentsAppService = productCommentsAppService;
        }

        public async Task OnGetAsync()
        {
            var productCommentWithNavigationPropertiesDto = await _productCommentsAppService.GetWithNavigationPropertiesAsync(Id);
            ProductComment = ObjectMapper.Map<ProductCommentDto, ProductCommentUpdateDto>(productCommentWithNavigationPropertiesDto.ProductComment);

            Product = productCommentWithNavigationPropertiesDto.Product;

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            await _productCommentsAppService.UpdateAsync(Id, ProductComment);
            return NoContent();
        }
    }
}