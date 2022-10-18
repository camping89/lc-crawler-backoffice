using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using WordPressPCL;
using WordPressPCL.Models;
using WooCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerAloBacSi : DomainService
{
    private readonly ICategoryAloBacSiRepository _categoryAloBacSiRepository;
    private readonly IArticleAloBacSiRepository  _articleAloBacSiRepository;
    private readonly IMediaAloBacSiRepository    _mediaAloBacSiRepository;
    private readonly MediaManagerAloBacSi        _mediaManagerAloBacSi;
    private readonly IDataSourceRepository       _dataSourceRepository;
    private          string                      BASEURL = string.Empty;
    private          DataSource                  _dataSource;

    public WordpressManagerAloBacSi(ICategoryAloBacSiRepository categoryAloBacSiRepository, IArticleAloBacSiRepository articleAloBacSiRepository, IMediaAloBacSiRepository mediaAloBacSiRepository, MediaManagerAloBacSi mediaManagerAloBacSi,
                                    IDataSourceRepository       dataSourceRepository)
    {
        _categoryAloBacSiRepository = categoryAloBacSiRepository;
        _articleAloBacSiRepository  = articleAloBacSiRepository;
        _mediaAloBacSiRepository    = mediaAloBacSiRepository;
        _mediaManagerAloBacSi       = mediaManagerAloBacSi;
        _dataSourceRepository       = dataSourceRepository;
    }

    public async Task DoSyncToWordpress()
    {
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.AloBacSiUrl));
        if (_dataSource == null)
        {
            return;
        }
        BASEURL = _dataSource.PostToSite;
        
        var articles = await _articleAloBacSiRepository.GetListWithNavigationPropertiesAsync();
        foreach (var article in articles.Where(x=>x.Article.LastSyncedAt.HasValue == false))
        {
            await PostToWPAsync(article);
        }
    }

    private async Task PostToWPAsync(ArticleWithNavigationProperties articleNav)
    {
        var featureMedia = await PostFeatureMediasAsync( articleNav.Media);
        var postMedias = await PostMediasAsync(articleNav.Article);

        var post = ConvertToPost(articleNav, featureMedia, postMedias);
        
        //pass the Wordpress REST API base address as string
       
        var client = await InitClient();
        
        //Category
        if (articleNav.Categories != null)
        {
            var wooCategories = await client.Categories.GetAsync(useAuth:true);
            if (wooCategories != null)
            {
                foreach (var category in articleNav.Categories)
                {
                    var wooCategory = wooCategories.FirstOrDefault(x => x.Name.Equals(category.Name));
                    if (wooCategory == null)
                    {
                        wooCategory = await client.Categories.CreateAsync(new WooCategory()
                        {
                            Name = category.Name,
                            Slug = category.Slug
                        });
                    }

                    if (wooCategory != null)
                    {
                        post.Categories.Add(wooCategory.Id);
                    }
                }
            }
            
            var result = await client.Posts.CreateAsync(post);
            if (result != null)
            {
                articleNav.Article.LastSyncedAt = DateTime.UtcNow;
                await _articleAloBacSiRepository.UpdateAsync(articleNav.Article, true);
            }
        }
    }

    private Post ConvertToPost(ArticleWithNavigationProperties articleNav,MediaItem featureMedia, List<MediaItem> contentMedias)
    {
        var article = articleNav.Article;
        article.Content = ReplaceImageUrls(article.Content, contentMedias);
        
        var post = new Post()
        {
            Title = new Title( article.Title),
            Content = new Content( article.Content),
            Date = article.CreatedAt,
            Excerpt = new Excerpt(article.Excerpt),
            Status = Status.Pending,
            LiveblogLikes = article.LikeCount,
            CommentStatus = OpenStatus.Open,
            FeaturedMedia = featureMedia?.Id,
            Categories = new List<int>()
        };
        
        return post;
    }
    
    private string ReplaceImageUrls(string contentHtml, List<MediaItem> medias)
    {
        try
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(contentHtml);
            if (htmlDoc.DocumentNode != null)
            {
                foreach (var node in htmlDoc.DocumentNode.Descendants("//img"))
                {
                    var mediaIdAttributeValue = node.Attributes["@media-id"].Value;
                    if (!string.IsNullOrEmpty(mediaIdAttributeValue))
                    {
                        mediaIdAttributeValue = mediaIdAttributeValue.Split("/").LastOrDefault();
                
                        var media = medias.FirstOrDefault(x => mediaIdAttributeValue != null && x.Title != null && x.Title.Raw != null && x.Title.Raw.Contains(mediaIdAttributeValue));
            
                        if (media != null)
                        {
                            node.SetAttributeValue("src", media.SourceUrl );
                        }
                    }
            
                }

                var newHtml = htmlDoc.DocumentNode.WriteTo();
                return newHtml;
            }
            return contentHtml;
        }
        catch (Exception e)
        {
            return contentHtml;
        }
    }


    private async Task<List<MediaItem>> PostMediasAsync(Article article)
    {
        if (article is { Medias: { } })
        {
            var mediaIds = article.Medias.Select(x => x.MediaId).ToList();
            var medias = await _mediaAloBacSiRepository.GetListAsync(x =>mediaIds.Contains(x.Id));
            //pass the Wordpress REST API base address as string
            var client =await InitClient();
        
            if (medias != null)
            {
                var mediaItems = new List<MediaItem>();
                foreach (var media in medias)
                {
                    try
                    {
                        var stream = await _mediaManagerAloBacSi.GetFileStream(media.Name);
                        mediaItems.Add(await client.Media.CreateAsync(stream,media.Name, media.ContentType));
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }

                return mediaItems;
            }

        }
        return null;
    }
    
    private async Task<MediaItem> PostFeatureMediasAsync(Media media)
    {
        //pass the Wordpress REST API base address as string
        var client = await InitClient();

        if (media != null)
        {
            var stream = await _mediaManagerAloBacSi.GetFileStream(media.Name);
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