using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.Domain.Services;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Enums;
using WordPressPCL;
using WordPressPCL.Models;
using WordpresCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerLongChau : DomainService
{
    private readonly IArticleLongChauRepository _articleLongChauRepository;
    private readonly IMediaLongChauRepository _mediaLongChauRepository;
    private readonly MediaManagerLongChau _mediaManagerLongChau;
    private readonly ICategoryLongChauRepository _categoryLongChauRepository;
    private readonly IDataSourceRepository _dataSourceRepository;
    private string BASEURL = string.Empty;
    private DataSource _dataSource;

    public WordpressManagerLongChau(IArticleLongChauRepository articleLongChauRepository, IMediaLongChauRepository mediaLongChauRepository, MediaManagerLongChau mediaManagerLongChau, IDataSourceRepository dataSourceRepository, ICategoryLongChauRepository categoryLongChauRepository)
    {
        _articleLongChauRepository = articleLongChauRepository;
        _mediaLongChauRepository = mediaLongChauRepository;
        _mediaManagerLongChau = mediaManagerLongChau;
        _dataSourceRepository = dataSourceRepository;
        _categoryLongChauRepository = categoryLongChauRepository;
    }

    public async Task DoSyncToWordpress()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null)
        {
            return;
        }

        BASEURL = _dataSource.PostToSite;

        var articles = await _articleLongChauRepository.GetListWithNavigationPropertiesAsync();

        var client = await InitClient();
        var wooCategories = (await client.Categories.GetAllAsync(useAuth: true)).ToList();

        foreach (var article in articles)
        {
            await PostToWPAsync(wooCategories, article);
        }
    }

    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (_dataSource == null)
        {
            return;
        }

        BASEURL = _dataSource.PostToSite;

        var categories = (await _categoryLongChauRepository.GetListAsync(x => x.CategoryType == CategoryType.Article)).Select(x => x.Name).Distinct().ToList();
        //Category
        var client = await InitClient();
        var wooCategories = (await client.Categories.GetAllAsync(useAuth: true)).ToList();

        foreach (var cateStr in categories)
        {
            if (cateStr.IsNullOrEmpty())
            {
                continue;
            }

            var categoriesTerms = cateStr.Split("->").ToList();

            var cateName = categoriesTerms.FirstOrDefault()?.Trim().Replace("&", "&amp;");
            var wooRootCategory = wooCategories.FirstOrDefault(x => x.Name.Equals(cateName, StringComparison.InvariantCultureIgnoreCase));
            if (wooRootCategory == null)
            {
                try
                {
                    var cateNew = new WordpresCategory
                    {
                        Name = cateName
                    };
                    wooRootCategory = await client.Categories.CreateAsync(cateNew);
                    wooCategories.Add(wooRootCategory);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            if (categoriesTerms.Count > 1 && wooRootCategory != null)
            {
                var cateParent = wooRootCategory;
                for (var i = 1; i < categoriesTerms.Count; i++)
                {
                    try
                    {
                        var subCateName = categoriesTerms[i].Trim().Replace("&", "&amp;");

                        var wooSubCategory = wooCategories.FirstOrDefault(x => x.Name.Equals(subCateName, StringComparison.InvariantCultureIgnoreCase));
                        if (wooSubCategory == null)
                        {
                            var cateNew = new WordpresCategory
                            {
                                Name = subCateName,
                                Parent = cateParent.Id
                            };

                            cateNew = await client.Categories.CreateAsync(cateNew);
                            wooCategories.Add(cateNew);

                            cateParent = cateNew;
                        }
                        else
                        {
                            cateParent = wooSubCategory;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }

    private async Task PostToWPAsync(List<WordpresCategory> wpCategories, ArticleWithNavigationProperties articleNav)
    {
        var featureMedia = await PostFeatureMediasAsync(articleNav.Media);
        var postMedias = await PostMediasAsync(articleNav.Article);

        var post = ConvertToPost(articleNav, featureMedia, postMedias);

        if (articleNav.Categories != null)
        {
            foreach (var articleCategory in articleNav.Categories)
            {
                var encodeName = articleCategory.Name.Split("->").LastOrDefault()?.Replace("&","&amp;").Trim();
                var wpCate = wpCategories.FirstOrDefault(x => x.Name.Contains(encodeName, StringComparison.InvariantCultureIgnoreCase));
                if (wpCate != null)
                {
                    post.Categories.Add(wpCate.Id);
                }
            }
        }
        //pass the Wordpress REST API base address as string

        var client = await InitClient();
        var result = await client.Posts.CreateAsync(post);
        if (result != null)
        {
            articleNav.Article.LastSyncedAt = DateTime.UtcNow;
            await _articleLongChauRepository.UpdateAsync(articleNav.Article, true);
        }
    }

    private Post ConvertToPost(ArticleWithNavigationProperties articleNav, MediaItem featureMedia, List<MediaItem> contentMedias)
    {
        var article = articleNav.Article;
        article.Content = ReplaceImageUrls(article.Content, contentMedias);

        var post = new Post()
        {
            Title = new Title(article.Title),
            Content = new Content(article.Content),
            Date = article.CreatedAt,
            Excerpt = new Excerpt(article.Excerpt),
            Status = Status.Pending,
            LiveblogLikes = article.LikeCount,
            CommentStatus = OpenStatus.Open,
            FeaturedMedia = featureMedia?.Id
        };

        return post;
    }

    private string ReplaceImageUrls(string contentHtml, List<MediaItem> medias)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(contentHtml);
        foreach (var node in htmlDoc.DocumentNode.Descendants("img"))
        {
            var mediaIdAttributeValue = node.Attributes["@media-id"].Value;
            var media = medias.FirstOrDefault(x => mediaIdAttributeValue.Contains(x.Title.Raw));

            if (media != null)
            {
                node.SetAttributeValue("src", media.SourceUrl);
            }
        }

        var newHtml = htmlDoc.DocumentNode.WriteTo();
        return newHtml;
    }


    private async Task<List<MediaItem>> PostMediasAsync(Article article)
    {
        if (article is { Medias: { } })
        {
            var mediaIds = article.Medias.Select(x => x.MediaId).ToList();
            var medias = await _mediaLongChauRepository.GetListAsync(x => mediaIds.Contains(x.Id) && x.IsDowloaded);
            //pass the Wordpress REST API base address as string
            var client = await InitClient();

            if (medias != null)
            {
                var mediaItems = new List<MediaItem>();
                foreach (var media in medias)
                {
                    var stream = await _mediaManagerLongChau.GetFileStream(media.Name);
                    mediaItems.Add(await client.Media.CreateAsync(stream, media.Name, media.ContentType));
                }

                return mediaItems;
            }
        }

        return null;
    }

    private async Task<MediaItem> PostFeatureMediasAsync(Media media)
    {
        if (media is { IsDowloaded: true })
        {
            //pass the Wordpress REST API base address as string
            var client = await InitClient();

            var stream = await _mediaManagerLongChau.GetFileStream(media.Name);
            return await client.Media.CreateAsync(stream, media.Name, media.ContentType);
        }

        return null;
    }

    private Task<WordPressClient> InitClient()
    {
        //pass the Wordpress REST API base address as string
        var client = new WordPressClient($"{BASEURL}/wp-json/");
        client.Auth.UseBasicAuth(_dataSource.Configuration.Username, _dataSource.Configuration.Password);
        return Task.FromResult(client);
    }
}