using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Medias;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using LC.Crawler.BackOffice.Products;

namespace LC.Crawler.BackOffice.Web.Pages.Products
{
    public class EditModalModel : BackOfficePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public ProductUpdateDto Product { get; set; }

        public MediaDto Media { get; set; }
        public List<CategoryDto> Categories { get; set; }
        public List<MediaDto> Medias { get; set; }
        [BindProperty]
        public List<Guid> SelectedCategoryIds { get; set; }
        [BindProperty]
        public List<Guid> SelectedMediaIds { get; set; }
        public List<SelectListItem> DataSourceLookupListRequired { get; set; } = new List<SelectListItem>
        {
        };

        private readonly IProductsAppService _productsAppService;

        public EditModalModel(IProductsAppService productsAppService)
        {
            _productsAppService = productsAppService;
        }

        public async Task OnGetAsync()
        {
            var productWithNavigationPropertiesDto = await _productsAppService.GetWithNavigationPropertiesAsync(Id);
            Product = ObjectMapper.Map<ProductDto, ProductUpdateDto>(productWithNavigationPropertiesDto.Product);

            Media = productWithNavigationPropertiesDto.Media;
            Categories = productWithNavigationPropertiesDto.Categories;
            Medias = productWithNavigationPropertiesDto.Medias;
            DataSourceLookupListRequired.AddRange((
                        await _productsAppService.GetDataSourceLookupAsync(new LookupRequestDto
                        {
                            MaxResultCount = LimitedResultRequestDto.MaxMaxResultCount
                        })).Items.Select(t => new SelectListItem(t.DisplayName, t.Id.ToString())).ToList()
            );

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            Product.CategoryIds = SelectedCategoryIds;

            Product.MediaIds = SelectedMediaIds;

            await _productsAppService.UpdateAsync(Id, Product);
            return NoContent();
        }
    }
}