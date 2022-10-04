using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using LC.Crawler.BackOffice.ProductVariants;
using LC.Crawler.BackOffice.Shared;

namespace LC.Crawler.BackOffice.Web.Pages.ProductVariants
{
    public class IndexModel : AbpPageModel
    {
        public string SKUFilter { get; set; }
        public decimal? RetailPriceFilterMin { get; set; }

        public decimal? RetailPriceFilterMax { get; set; }
        public double? DiscountRateFilterMin { get; set; }

        public double? DiscountRateFilterMax { get; set; }
        public decimal? DiscountedPriceFilterMin { get; set; }

        public decimal? DiscountedPriceFilterMax { get; set; }
        [SelectItems(nameof(ProductLookupList))]
        public Guid ProductIdFilter { get; set; }
        public List<SelectListItem> ProductLookupList { get; set; } = new List<SelectListItem>
        {
            new SelectListItem(string.Empty, "")
        };

        private readonly IProductVariantsAppService _productVariantsAppService;

        public IndexModel(IProductVariantsAppService productVariantsAppService)
        {
            _productVariantsAppService = productVariantsAppService;
        }

        public async Task OnGetAsync()
        {
            ProductLookupList.AddRange((
                    await _productVariantsAppService.GetProductLookupAsync(new LookupRequestDto
                    {
                        MaxResultCount = LimitedResultRequestDto.MaxMaxResultCount
                    })).Items.Select(t => new SelectListItem(t.DisplayName, t.Id.ToString())).ToList()
            );

            await Task.CompletedTask;
        }
    }
}