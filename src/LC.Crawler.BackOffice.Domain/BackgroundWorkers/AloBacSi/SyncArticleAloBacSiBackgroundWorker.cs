using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.BackgroundWorkers.Hangfire;
using WordPressPCL;
using WordPressPCL.Models;
using WooCategory = WordPressPCL.Models.Category;
using Guid = System.Guid;

namespace LC.Crawler.BackOffice.BackgroundWorkers.AloBacSi;

public class SyncArticleAloBacSiBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly ICategoryAloBacSiRepository _categoryAloBacSiRepository;
    private readonly IArticleAloBacSiRepository _articleAloBacSiRepository;
    private readonly IMediaAloBacSiRepository _mediaAloBacSiRepository;
    private readonly MediaManagerAloBacSi _mediaManagerAloBacSi;
    private readonly IDataSourceRepository _dataSourceRepository;
    private string BASEURL = string.Empty;
    
    public SyncArticleAloBacSiBackgroundWorker(IArticleAloBacSiRepository articleAloBacSiRepository, IMediaAloBacSiRepository mediaAloBacSiRepository, MediaManagerAloBacSi mediaManagerAloBacSi, IDataSourceRepository dataSourceRepository, ICategoryAloBacSiRepository categoryAloBacSiRepository)
    {
        _articleAloBacSiRepository = articleAloBacSiRepository;
        _mediaAloBacSiRepository = mediaAloBacSiRepository;
        _mediaManagerAloBacSi = mediaManagerAloBacSi;
        _dataSourceRepository = dataSourceRepository;
        _categoryAloBacSiRepository = categoryAloBacSiRepository;

        RecurringJobId            = nameof(SyncArticleAloBacSiBackgroundWorker);
        CronExpression            = Cron.Daily(0,0);
    }

    public override async Task DoWorkAsync()
    {
        var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.AloBacSiUrl));
        if (dataSource == null)
        {
            return;
        }
        BASEURL = dataSource.PostToSite;
        
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
        client.Auth.UseBasicAuth("admin", "123456");
        return Task.FromResult(client);
    }
}