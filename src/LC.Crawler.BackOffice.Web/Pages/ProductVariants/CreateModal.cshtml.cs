using LC.Crawler.BackOffice.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LC.Crawler.BackOffice.ProductVariants;

namespace LC.Crawler.BackOffice.Web.Pages.ProductVariants
{
    public class CreateModalModel : BackOfficePageModel
    {
        [BindProperty]
        public ProductVariantCreateDto ProductVariant { get; set; }

        public List<SelectListItem> ProductLookupListRequired { get; set; } = new List<SelectListItem>
        {
        };

        private readonly IProductVariantsAppService _productVariantsAppService;

        public CreateModalModel(IProductVariantsAppService productVariantsAppService)
        {
            _productVariantsAppService = productVariantsAppService;
        }

        public async Task OnGetAsync()
        {
            ProductVariant = new ProductVariantCreateDto();
            ProductLookupListRequired.AddRange((
                                    await _productVariantsAppService.GetProductLookupAsync(new LookupRequestDto
                                    {
                                        MaxResultCount = LimitedResultRequestDto.MaxMaxResultCount
                                    })).Items.Select(t => new SelectListItem(t.DisplayName, t.Id.ToString())).ToList()
                        );

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            await _productVariantsAppService.CreateAsync(ProductVariant);
            return NoContent();
        }
    }
}