using System;
using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.CrawlerAccounts
{
    public class CrawlerAccountDto : AuditedEntityDto<Guid>, IHasConcurrencyStamp
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string TwoFactorCode { get; set; }
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public AccountType AccountType { get; set; }
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public AccountStatus AccountStatus { get; set; }
        public string Email { get; set; }
        public string EmailPassword { get; set; }
        public bool IsActive { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}