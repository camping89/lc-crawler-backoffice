using LC.Crawler.BackOffice.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Shared;

namespace LC.Crawler.BackOffice.Web.Pages.Categories
{
    public class IndexModel : AbpPageModel
    {
        public string NameFilter { get; set; }
        public string SlugFilter { get; set; }
        public string DescriptionFilter { get; set; }
        public CategoryType? CategoryTypeFilter { get; set; }

        private readonly ICategoriesAppService _categoriesAppService;

        public IndexModel(ICategoriesAppService categoriesAppService)
        {
            _categoriesAppService = categoriesAppService;
        }

        public async Task OnGetAsync()
        {

            await Task.CompletedTask;
        }
    }
}