using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.CrawlerProxies
{
    public class CrawlerProxyDto : AuditedEntityDto<Guid>, IHasConcurrencyStamp
    {
        public string Ip { get; set; }
        public string Port { get; set; }
        public string Protocol { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime? PingedAt { get; set; }
        public bool IsActive { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}