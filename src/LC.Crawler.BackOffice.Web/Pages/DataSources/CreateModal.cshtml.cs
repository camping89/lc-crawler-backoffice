using LC.Crawler.BackOffice.Shared;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LC.Crawler.BackOffice.DataSources;

namespace LC.Crawler.BackOffice.Web.Pages.DataSources
{
    public class CreateModalModel : BackOfficePageModel
    {
        [BindProperty]
        public DataSourceCreateDto DataSource { get; set; }

        private readonly IDataSourcesAppService _dataSourcesAppService;

        public CreateModalModel(IDataSourcesAppService dataSourcesAppService)
        {
            _dataSourcesAppService = dataSourcesAppService;
        }

        public async Task OnGetAsync()
        {
            DataSource = new DataSourceCreateDto();

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            await _dataSourcesAppService.CreateAsync(DataSource);
            return NoContent();
        }
    }
}