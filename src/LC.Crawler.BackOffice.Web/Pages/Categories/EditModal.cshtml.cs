using LC.Crawler.BackOffice.Shared;
using LC.Crawler.BackOffice.Categories;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using LC.Crawler.BackOffice.Categories;

namespace LC.Crawler.BackOffice.Web.Pages.Categories
{
    public class EditModalModel : BackOfficePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CategoryUpdateDto Category { get; set; }

        public CategoryDto Category1 { get; set; }

        private readonly ICategoriesAppService _categoriesAppService;

        public EditModalModel(ICategoriesAppService categoriesAppService)
        {
            _categoriesAppService = categoriesAppService;
        }

        public async Task OnGetAsync()
        {
            var categoryWithNavigationPropertiesDto = await _categoriesAppService.GetWithNavigationPropertiesAsync(Id);
            Category = ObjectMapper.Map<CategoryDto, CategoryUpdateDto>(categoryWithNavigationPropertiesDto.Category);

            Category1 = categoryWithNavigationPropertiesDto.Category1;

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            await _categoriesAppService.UpdateAsync(Id, Category);
            return NoContent();
        }
    }
}