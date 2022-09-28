using Volo.Abp.Application.Dtos;
using System;

namespace LC.Crawler.BackOffice.CrawlerProxies
{
    public class GetCrawlerProxiesInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string Ip { get; set; }
        public string Port { get; set; }
        public string Protocol { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime? PingedAtMin { get; set; }
        public DateTime? PingedAtMax { get; set; }
        public bool? IsActive { get; set; }

        public GetCrawlerProxiesInput()
        {

        }
    }
}