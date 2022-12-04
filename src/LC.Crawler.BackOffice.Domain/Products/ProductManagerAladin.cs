using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Helpers;
using LC.Crawler.BackOffice.Medias;
using LC.Crawler.BackOffice.Payloads;
using LC.Crawler.BackOffice.ProductAttributes;
using LC.Crawler.BackOffice.ProductComments;
using LC.Crawler.BackOffice.ProductReviews;
using LC.Crawler.BackOffice.ProductVariants;
using LC.Crawler.BackOffice.TrackingDataSources;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace LC.Crawler.BackOffice.Products;

public class ProductManagerAladin : DomainService
{
    private readonly IProductAladinRepository _productAladinRepository;
    private readonly ICategoryAladinRepository _categoryAladinRepository;
    private readonly IMediaAladinRepository _mediaAladinRepository;
    private readonly IProductAttributeAladinRepository _productAttributeAladinRepository;
    private readonly IProductVariantAladinRepository _productVariantAladinRepository;
    private readonly IDataSourceRepository _dataSourceRepository;
    private readonly ITrackingDataSourceRepository _trackingDataSourceRepository;
    
    private readonly IProductReviewAladinRepository _productReviewAladinRepository;
    private readonly IProductCommentAladinRepository _productCommentAladinRepository;

    public ProductManagerAladin(IProductAladinRepository productAladinRepository, ICategoryAladinRepository categoryAladinRepository, IMediaAladinRepository mediaAladinRepository, IDataSourceRepository dataSourceRepository, IProductAttributeAladinRepository productAttributeAladinRepository,
        IProductVariantAladinRepository productVariantAladinRepository, ITrackingDataSourceRepository trackingDataSourceRepository,
        IProductReviewAladinRepository productReviewAladinRepository,
        IProductCommentAladinRepository productCommentAladinRepository)
    {
        _productAladinRepository = productAladinRepository;
        _categoryAladinRepository = categoryAladinRepository;
        _mediaAladinRepository = mediaAladinRepository;
        _dataSourceRepository = dataSourceRepository;
        _productAttributeAladinRepository = productAttributeAladinRepository;
        _productVariantAladinRepository = productVariantAladinRepository;
        _trackingDataSourceRepository = trackingDataSourceRepository;
        _productReviewAladinRepository = productReviewAladinRepository;
        _productCommentAladinRepository = productCommentAladinRepository;
    }

    public async  Task ProcessingDataAsync(CrawlEcommercePayload ecommercePayload)
    {
        var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AladinUrl));
        if (dataSource == null)
        {
            return;
        }
        var categories = await _categoryAladinRepository.GetListAsync(_ => _.CategoryType == CategoryType.Ecom);
        var medias = await _mediaAladinRepository.GetListAsync();
        foreach (var rawProducts in ecommercePayload.Products.GroupBy(_ => _.Url))
        {
            try
            {
                var rawProduct = rawProducts.First();
                if (rawProduct.Code is null && rawProduct.Title is null)
                {
                    continue;
                }
                
                if (AbpStringExtensions.IsNullOrEmpty(rawProduct.Code))
                {
                    await _trackingDataSourceRepository.InsertAsync(new TrackingDataSource()
                    {
                        Url = rawProduct.Url,
                        CrawlType = CrawlType.Ecom,
                        PageDataSource = PageDataSource.Aladin,
                        Error = TrackingDataSourceConsts.EmptyCode
                    }, true);
                }

                #region Update product

                var productExist = await _productAladinRepository.FirstOrDefaultAsync(x => x.Code == rawProduct.Code);
                if (productExist != null)
                {
                    productExist.Name = rawProduct.Title;
                    productExist.Brand = rawProduct.Brand;
                    productExist.Tags  = rawProduct.Tags;
                    
                    if (string.IsNullOrEmpty(productExist.Url))
                    {
                        productExist.Url = rawProduct.Url;
                    }
                    
                    var attributes = await _productAttributeAladinRepository.GetListAsync(_ => _.ProductId == productExist.Id);

                    //Init new attribute from raw product not in db
                    foreach (var rawAttribute in from rawAttribute in rawProduct.Attributes
                             let attribute =
                                 attributes.Where(_ => _.Key == rawAttribute.Key && _.Value == rawAttribute.Value)
                             where !attribute.IsNotNullOrEmpty()
                             select rawAttribute)
                    {
                        await _productAttributeAladinRepository.InsertAsync(new ProductAttribute()
                        {
                            Key = rawAttribute.Key,
                            Slug = rawAttribute.Slug,
                            Value = rawAttribute.Value,
                            ProductId = productExist.Id
                        }, true);
                    }

                    //Delete attribute from db not in raw product
                    foreach (var attribute in from attribute in attributes
                             let rawAttribute =
                                 rawProduct.Attributes.Where(_ => _.Key == attribute.Key && _.Value == attribute.Value)
                             where rawAttribute.IsNullOrEmpty()
                             select attribute)
                    {
                        await _productAttributeAladinRepository.DeleteAsync(attribute);
                    }

                    //Update price 
                    if (rawProduct.Variants != null)
                    {
                        foreach (var variant in rawProduct.Variants)
                        {
                            var productVariant = await _productVariantAladinRepository.FirstOrDefaultAsync(x => x.ProductId == productExist.Id && x.SKU == variant.SKU);
                            if (productVariant != null)
                            {
                                productVariant.DiscountedPrice = variant.DiscountedPrice;
                                productVariant.DiscountRate = variant.DiscountRate;
                                productVariant.RetailPrice = variant.RetailPrice;
                                await _productVariantAladinRepository.UpdateAsync(productVariant,true);
                            }
                            else
                            {
                                await _productVariantAladinRepository.InsertAsync(new ProductVariant()
                                {
                                    ProductId = productExist.Id,
                                    SKU = variant.SKU,
                                    DiscountedPrice = variant.DiscountedPrice,
                                    DiscountRate = variant.DiscountRate,
                                    RetailPrice = variant.RetailPrice
                                },true);
                            }
                        }
                    }
                    
                    //ProductReviews
                    if (rawProduct.Reviews != null)
                    {
                        var productReviews = await _productReviewAladinRepository.GetListAsync(x => x.ProductId == productExist.Id);
                        foreach (var review in rawProduct.Reviews.Where(x=> productReviews.All(pr=>pr.Name != x.Name)))
                        {
                            await _productReviewAladinRepository.InsertAsync(new ProductReview()
                            {
                                Name = review.Name,
                                Content = review.Content,
                                Rating = review.Rating,
                                Likes = review.Likes,
                                ProductId = productExist.Id,
                                CreatedAt = DateTime.UtcNow
                            },true);
                        }
                    }

                    //ProductComments
                    if (rawProduct.Comments != null)
                    {
                        var productComments = await _productCommentAladinRepository.GetListAsync(x => x.ProductId == productExist.Id);
                        foreach (var comment in rawProduct.Comments.Where(x=> productComments.All(pr=>pr.Name != x.Name)))
                        {
                            await _productCommentAladinRepository.InsertAsync(new ProductComment()
                            {
                                Name = comment.Name,
                                Content = comment.Content,
                                Likes = comment.Likes,
                                ProductId = productExist.Id,
                                CreatedAt = DateTime.UtcNow
                            },true);
                        }
                    }
                    
                    //ProductDescription
                    var mediaUrls = rawProduct.Description.GetImageUrls();
                    var productMedias = medias.Where(_ => mediaUrls.Any(x => StringHtmlHelper.CompareUrls(_.Url, x))).ToList();
                    if (mediaUrls.Any())
                    {
                        var newMedias = mediaUrls.Where(_ => !productMedias.Any(x => StringHtmlHelper.CompareUrls(_, x.Url))).Select(url => new Media()
                        {
                            Url         = url,
                            IsDowloaded = false
                        }).ToList();

                        if(newMedias.IsNotNullOrEmpty()) await _mediaAladinRepository.InsertManyAsync(newMedias, true);
                        
                        productMedias.AddRange(newMedias);

                        productExist.Description = StringHtmlHelper.SetContentMediaIds(rawProduct.Description, productMedias);

                        foreach (var media in newMedias)
                        {
                            productExist.Medias.Add(new ProductMedia(productExist.Id, media.Id));
                        }
                    }
                    else
                    {
                        productExist.Description = rawProduct.Description;
                    }
                    
                    await _productAladinRepository.UpdateAsync(productExist, true);
                    continue;
                }

                #endregion


                #region Add new product

                var product = new Product(GuidGenerator.Create())
                {
                    Name             = rawProduct.Title,
                    Code             = rawProduct.Code,
                    Description      = rawProduct.Description,
                    ShortDescription = rawProduct.ShortDescription,
                    DataSourceId     = dataSource.Id,
                    Brand            = rawProduct.Brand,
                    Tags             = rawProduct.Tags,
                    Url              = rawProduct.Url
                };
               
                foreach (var raw in rawProducts)
                {
                    var category = categories.FirstOrDefault(x => x.Name == raw.Category);
                    if (category == null)
                    {
                        category = new Category()
                        {
                            Name = raw.Category
                        };
                        await _categoryAladinRepository.InsertAsync(category, true);
                        categories.Add(category);
                    }
                    product.AddCategory(category.Id);
                }

                // Images
                var newProductMedias = new List<Media>();
                if (rawProduct.ImageUrls != null)
                {
                    newProductMedias = await CreateMediasAsync(rawProduct.ImageUrls);
                    foreach (var media in newProductMedias)
                    {
                        product.AddMedia(media.Id);
                    }
                }
                
                if (!string.IsNullOrEmpty(rawProduct.Description))
                {
                    var mediaUrls = rawProduct.Description.GetImageUrls();

                    if (mediaUrls.Any())
                    {
                        var newMedias = mediaUrls
                            .Where(_ => !newProductMedias.Any(x => StringHtmlHelper.CompareUrls(_, x.Url))).Select(url => new Media()
                            {
                                Url = url,
                                IsDowloaded = false
                            }).ToList();
                        
                        if(newMedias.IsNotNullOrEmpty()) await _mediaAladinRepository.InsertManyAsync(newMedias, true);
                        
                        newProductMedias.AddRange(newProductMedias);

                        product.Description = StringHtmlHelper.SetContentMediaIds(rawProduct.Description, newProductMedias);
                            
                        foreach (var media in newProductMedias)
                        {
                            product.AddMedia(media.Id);
                        }
                    }
                }
                
                //Variants
                if (rawProduct.Variants != null)
                {
                    foreach (var variant in rawProduct.Variants)
                    {
                        await _productVariantAladinRepository.InsertAsync(new ProductVariant()
                        {
                            ProductId = product.Id,
                            SKU = variant.SKU,
                            DiscountedPrice = variant.DiscountedPrice,
                            DiscountRate = variant.DiscountRate,
                            RetailPrice = variant.RetailPrice
                        },true);
                    }
                }
                
                //Attributes
                if (rawProduct.Attributes != null)
                {
                    foreach (var attribute in rawProduct.Attributes)
                    {
                        await _productAttributeAladinRepository.InsertAsync(new ProductAttribute()
                        {
                            Key = attribute.Key,
                            Slug = attribute.Slug,
                            Value = attribute.Value,
                            ProductId = product.Id
                        },true);
                    }
                }
                
                
                //ProductReviews
                if (rawProduct.Reviews != null)
                {
                    foreach (var review in rawProduct.Reviews)
                    {
                        await _productReviewAladinRepository.InsertAsync(new ProductReview()
                        {
                            Name = review.Name,
                            Content = review.Content,
                            Rating = review.Rating,
                            Likes = review.Likes,
                            ProductId = product.Id,
                            CreatedAt = DateTime.UtcNow
                        },true);
                    }
                }

                //ProductComments
                if (rawProduct.Comments != null)
                {
                    foreach (var comment in rawProduct.Comments)
                    {
                        await _productCommentAladinRepository.InsertAsync(new ProductComment()
                        {
                            Name = comment.Name,
                            Content = comment.Content,
                            Likes = comment.Likes,
                            ProductId = product.Id,
                            CreatedAt = DateTime.UtcNow
                        },true);
                    }
                }


                await _productAladinRepository.InsertAsync(product, true);
                
                await CheckFormatEntity(product);

                #endregion
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
    
    /// <summary>
    /// Remove the entity in case having format exception (unicode types ...)
    /// </summary>
    /// <param name="product"></param>
    private async Task CheckFormatEntity(Product product)
    {
        try
        {
            var checkProduct = await _productAladinRepository.GetAsync(product.Id);
        }
        catch (Exception e)
        {
            if (e.GetType() == typeof(FormatException))
            {
                Logger.LogException(e);
                await _productAladinRepository.DeleteAsync(product.Id);
            }
        }
    }

    private async Task<List<Media>> CreateMediasAsync(List<string> urls)
    {
        var medias = urls.Select(x => new Media()
        {
            Url = x,
            IsDowloaded = false
        }).ToList();

        if (medias.IsNotNullOrEmpty())
        {
            await _mediaAladinRepository.InsertManyAsync(medias,true);
        }
        return medias;
    }
    
    public async Task<List<KeyValuePair<string, int>>> CountProductByCategory()
    {
        var products = await _productAladinRepository.GetListAsync();
        var categories = await _categoryAladinRepository.GetListAsync(_ => _.CategoryType == CategoryType.Ecom);
        return categories.Select(category => new KeyValuePair<string, int>(category.Name, products.Count(_ => _.Categories.Select(c => c.CategoryId).Contains(category.Id)))).ToList();
    }
}