using System.ComponentModel.DataAnnotations;
using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Domain.Entities;

namespace LC.Crawler.BackOffice.CrawlerAccounts
{
    public class CrawlerAccountUpdateDto : IHasConcurrencyStamp
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string TwoFactorCode { get; set; }
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public AccountType AccountType { get; set; }
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public AccountStatus AccountStatus { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string EmailPassword { get; set; }
        public bool IsActive { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}