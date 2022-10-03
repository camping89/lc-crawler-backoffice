using LC.Crawler.BackOffice.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LC.Crawler.BackOffice.Products;

namespace LC.Crawler.BackOffice.Web.Pages.Products
{
    public class CreateModalModel : BackOfficePageModel
    {
        [BindProperty]
        public ProductCreateDto Product { get; set; }

        [BindProperty]
        public List<Guid> SelectedCategoryIds { get; set; }
        [BindProperty]
        public List<Guid> SelectedMediaIds { get; set; }

        private readonly IProductsAppService _productsAppService;

        public CreateModalModel(IProductsAppService productsAppService)
        {
            _productsAppService = productsAppService;
        }

        public async Task OnGetAsync()
        {
            Product = new ProductCreateDto();

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            Product.CategoryIds = SelectedCategoryIds;

            Product.MediaIds = SelectedMediaIds;

            await _productsAppService.CreateAsync(Product);
            return NoContent();
        }
    }
}