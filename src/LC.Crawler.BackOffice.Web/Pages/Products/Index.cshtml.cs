using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.Bootstrap.TagHelpers.Form;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.Shared;

namespace LC.Crawler.BackOffice.Web.Pages.Products
{
    public class IndexModel : AbpPageModel
    {
        public string NameFilter { get; set; }
        public string CodeFilter { get; set; }
        public string ShortDescriptionFilter { get; set; }
        public string DescriptionFilter { get; set; }
        public int? ExternalIdFilterMin { get; set; }

        public int? ExternalIdFilterMax { get; set; }
        [SelectItems(nameof(DataSourceLookupList))]
        public Guid DataSourceIdFilter { get; set; }
        public List<SelectListItem> DataSourceLookupList { get; set; } = new List<SelectListItem>
        {
            new SelectListItem(string.Empty, "")
        };

        private readonly IProductsAppService _productsAppService;

        public IndexModel(IProductsAppService productsAppService)
        {
            _productsAppService = productsAppService;
        }

        public async Task OnGetAsync()
        {
            DataSourceLookupList.AddRange((
                    await _productsAppService.GetDataSourceLookupAsync(new LookupRequestDto
                    {
                        MaxResultCount = LimitedResultRequestDto.MaxMaxResultCount
                    })).Items.Select(t => new SelectListItem(t.DisplayName, t.Id.ToString())).ToList()
            );

            await Task.CompletedTask;
        }
    }
}