using LC.Crawler.BackOffice.Shared;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using LC.Crawler.BackOffice.ProductVariants;

namespace LC.Crawler.BackOffice.Web.Pages.ProductVariants
{
    public class EditModalModel : BackOfficePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public ProductVariantUpdateDto ProductVariant { get; set; }

        public List<SelectListItem> ProductLookupListRequired { get; set; } = new List<SelectListItem>
        {
        };

        private readonly IProductVariantsAppService _productVariantsAppService;

        public EditModalModel(IProductVariantsAppService productVariantsAppService)
        {
            _productVariantsAppService = productVariantsAppService;
        }

        public async Task OnGetAsync()
        {
            var productVariantWithNavigationPropertiesDto = await _productVariantsAppService.GetWithNavigationPropertiesAsync(Id);
            ProductVariant = ObjectMapper.Map<ProductVariantDto, ProductVariantUpdateDto>(productVariantWithNavigationPropertiesDto.ProductVariant);

            ProductLookupListRequired.AddRange((
                                    await _productVariantsAppService.GetProductLookupAsync(new LookupRequestDto
                                    {
                                        MaxResultCount = LimitedResultRequestDto.MaxMaxResultCount
                                    })).Items.Select(t => new SelectListItem(t.DisplayName, t.Id.ToString())).ToList()
                        );

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            await _productVariantsAppService.UpdateAsync(Id, ProductVariant);
            return NoContent();
        }
    }
}