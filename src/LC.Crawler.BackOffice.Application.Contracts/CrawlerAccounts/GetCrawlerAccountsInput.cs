using LC.Crawler.BackOffice.Enums;
using Volo.Abp.Application.Dtos;

namespace LC.Crawler.BackOffice.CrawlerAccounts
{
    public class GetCrawlerAccountsInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string TwoFactorCode { get; set; }
        public AccountType? AccountType { get; set; }
        public AccountStatus? AccountStatus { get; set; }
        public string Email { get; set; }
        public string EmailPassword { get; set; }
        public bool? IsActive { get; set; }

        public GetCrawlerAccountsInput()
        {

        }
    }
}