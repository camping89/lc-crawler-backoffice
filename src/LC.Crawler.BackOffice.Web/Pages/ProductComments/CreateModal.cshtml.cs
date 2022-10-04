using LC.Crawler.BackOffice.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LC.Crawler.BackOffice.ProductComments;

namespace LC.Crawler.BackOffice.Web.Pages.ProductComments
{
    public class CreateModalModel : BackOfficePageModel
    {
        [BindProperty]
        public ProductCommentCreateDto ProductComment { get; set; }

        private readonly IProductCommentsAppService _productCommentsAppService;

        public CreateModalModel(IProductCommentsAppService productCommentsAppService)
        {
            _productCommentsAppService = productCommentsAppService;
        }

        public async Task OnGetAsync()
        {
            ProductComment = new ProductCommentCreateDto();

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            await _productCommentsAppService.CreateAsync(ProductComment);
            return NoContent();
        }
    }
}