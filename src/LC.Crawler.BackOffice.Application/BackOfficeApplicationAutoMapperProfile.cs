using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.CrawlerCredentials;
using LC.Crawler.BackOffice.CrawlerProxies;
using System;
using LC.Crawler.BackOffice.Shared;
using Volo.Abp.AutoMapper;
using LC.Crawler.BackOffice.CrawlerAccounts;
using AutoMapper;

namespace LC.Crawler.BackOffice;

public class BackOfficeApplicationAutoMapperProfile : Profile
{
    public BackOfficeApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        CreateMap<CrawlerAccount, CrawlerAccountDto>();

        CreateMap<CrawlerProxy, CrawlerProxyDto>();

        CreateMap<CrawlerCredential, CrawlerCredentialDto>();
        CreateMap<CrawlerCredentialWithNavigationProperties, CrawlerCredentialWithNavigationPropertiesDto>();
        CreateMap<CrawlerAccount, LookupDto<Guid?>>().ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Username));
        CreateMap<CrawlerProxy, LookupDto<Guid?>>().ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Ip));

        CreateMap<DataSource, DataSourceDto>();
    }
}