using Volo.Abp.Application.Dtos;
using System;

namespace LC.Crawler.BackOffice.DataSources
{
    public class GetDataSourcesInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string Url { get; set; }
        public bool? IsActive { get; set; }
        public string PostToSite { get; set; }

        public GetDataSourcesInput()
        {

        }
    }
}