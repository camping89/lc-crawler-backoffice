using LC.Crawler.BackOffice.ArticleComments;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.ProductAttributes;
using LC.Crawler.BackOffice.ProductVariants;
using LC.Crawler.BackOffice.Products;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.CrawlerCredentials;
using LC.Crawler.BackOffice.CrawlerProxies;
using System;
using System.Linq;
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
        CreateMap<Configuration, ConfigurationDto>().ReverseMap();

        CreateMap<Category, CategoryDto>();
        CreateMap<Category, CategoryResultDto>();

        CreateMap<CategoryWithNavigationProperties, CategoryWithNavigationPropertiesDto>();
        CreateMap<Category, LookupDto<Guid?>>().ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Name));

        CreateMap<Article, ArticleDto>();
        CreateMap<Article, ArticleResultDto>().Ignore(x => x.FeatureImageUrl).Ignore(x => x.Tags);

        CreateMap<ArticleWithNavigationProperties, ArticleWithNavigationPropertiesDto>();
        CreateMap<Category, LookupDto<Guid>>().ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Name));

        CreateMap<Media, MediaDto>();
        CreateMap<Media, MediaResultDto>().Ignore(x => x.Order);

        CreateMap<Product, ProductDto>();
        CreateMap<Product, ProductResultDto>().Ignore(x => x.FeatureImageUrl);

        CreateMap<ProductWithNavigationProperties, ProductWithNavigationPropertiesDto>();
        CreateMap<Media, LookupDto<Guid>>().ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Url));

        CreateMap<ProductVariant, ProductVariantDto>();
        CreateMap<ProductVariant, ProductVariantResultDto>();
        CreateMap<ProductVariantWithNavigationProperties, ProductVariantWithNavigationPropertiesDto>();
        CreateMap<Product, LookupDto<Guid>>().ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Name));

        CreateMap<ProductAttribute, ProductAttributeDto>();
        CreateMap<ProductAttribute, ProductAttributeResultDto>();

        CreateMap<ProductAttributeWithNavigationProperties, ProductAttributeWithNavigationPropertiesDto>();

        CreateMap<Media, LookupDto<Guid?>>().ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Url));

        CreateMap<DataSource, LookupDto<Guid>>().ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Url));

        CreateMap<ProductReview, ProductReviewDto>();
        CreateMap<ProductReview, ProductReviewResultDto>();

        CreateMap<ProductReviewWithNavigationProperties, ProductReviewWithNavigationPropertiesDto>();

        CreateMap<ProductComment, ProductCommentDto>();
        CreateMap<ProductComment, ProductCommentResultDto>();

        CreateMap<ProductCommentWithNavigationProperties, ProductCommentWithNavigationPropertiesDto>();

        CreateMap<ArticleComment, ArticleCommentDto>();
        CreateMap<ArticleComment, ArticleCommentResultDto>();

        CreateMap<ArticleCommentWithNavigationProperties, ArticleCommentWithNavigationPropertiesDto>();
        CreateMap<Article, LookupDto<Guid>>().ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Title));
    }
}