using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.CrawlerCredentials;
using LC.Crawler.BackOffice.CrawlerProxies;
using Volo.Abp.AutoMapper;
using LC.Crawler.BackOffice.CrawlerAccounts;
using AutoMapper;

namespace LC.Crawler.BackOffice.Web;

public class BackOfficeWebAutoMapperProfile : Profile
{
    public BackOfficeWebAutoMapperProfile()
    {
        //Define your object mappings here, for the Web project

        CreateMap<CrawlerAccountDto, CrawlerAccountUpdateDto>();

        CreateMap<CrawlerProxyDto, CrawlerProxyUpdateDto>();

        CreateMap<CrawlerCredentialDto, CrawlerCredentialUpdateDto>();

        CreateMap<DataSourceDto, DataSourceUpdateDto>();
    }
}