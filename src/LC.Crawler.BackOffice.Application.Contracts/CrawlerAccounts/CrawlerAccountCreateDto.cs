using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using LC.Crawler.BackOffice.Enums;

namespace LC.Crawler.BackOffice.CrawlerAccounts
{
    public class CrawlerAccountCreateDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string TwoFactorCode { get; set; }
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public AccountType AccountType { get; set; } = ((AccountType[])Enum.GetValues(typeof(AccountType)))[0];
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public AccountStatus AccountStatus { get; set; } = ((AccountStatus[])Enum.GetValues(typeof(AccountStatus)))[0];
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string EmailPassword { get; set; }
        public bool IsActive { get; set; }
    }
}