using LC.Crawler.BackOffice.ArticleComments;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.ProductVariants;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
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

        CreateMap<CategoryDto, CategoryUpdateDto>();

        CreateMap<ArticleDto, ArticleUpdateDto>().Ignore(x => x.CategoryIds);

        CreateMap<MediaDto, MediaUpdateDto>();

        CreateMap<ProductDto, ProductUpdateDto>().Ignore(x => x.CategoryIds).Ignore(x => x.MediaIds);

        CreateMap<ProductVariantDto, ProductVariantUpdateDto>();

        CreateMap<ArticleDto, ArticleUpdateDto>().Ignore(x => x.CategoryIds).Ignore(x => x.MediaIds);

        CreateMap<ProductReviewDto, ProductReviewUpdateDto>();

        CreateMap<ProductCommentDto, ProductCommentUpdateDto>();

        CreateMap<ArticleCommentDto, ArticleCommentUpdateDto>();
    }
}