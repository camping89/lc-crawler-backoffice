using LC.Crawler.BackOffice.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LC.Crawler.BackOffice.Categories;

namespace LC.Crawler.BackOffice.Web.Pages.Categories
{
    public class CreateModalModel : BackOfficePageModel
    {
        [BindProperty]
        public CategoryCreateDto Category { get; set; }

        private readonly ICategoriesAppService _categoriesAppService;

        public CreateModalModel(ICategoriesAppService categoriesAppService)
        {
            _categoriesAppService = categoriesAppService;
        }

        public async Task OnGetAsync()
        {
            Category = new CategoryCreateDto();

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            await _categoriesAppService.CreateAsync(Category);
            return NoContent();
        }
    }
}