using LC.Crawler.BackOffice.Shared;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;
using LC.Crawler.BackOffice.DataSources;

namespace LC.Crawler.BackOffice.Web.Pages.DataSources
{
    public class EditModalModel : BackOfficePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public DataSourceUpdateDto DataSource { get; set; }

        private readonly IDataSourcesAppService _dataSourcesAppService;

        public EditModalModel(IDataSourcesAppService dataSourcesAppService)
        {
            _dataSourcesAppService = dataSourcesAppService;
        }

        public async Task OnGetAsync()
        {
            var dataSource = await _dataSourcesAppService.GetAsync(Id);
            DataSource = ObjectMapper.Map<DataSourceDto, DataSourceUpdateDto>(dataSource);

        }

        public async Task<NoContentResult> OnPostAsync()
        {

            await _dataSourcesAppService.UpdateAsync(Id, DataSource);
            return NoContent();
        }
    }
}