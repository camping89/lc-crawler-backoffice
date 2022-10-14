using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.BackgroundWorkers.Hangfire;
using WordPressPCL;
using WordPressPCL.Models;

namespace LC.Crawler.BackOffice.BackgroundWorkers.LongChau;

public class SyncArticleLongChauBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly IArticleLongChauRepository _articleLongChauRepository;
    private readonly IMediaLongChauRepository _mediaLongChauRepository;
    private readonly MediaManagerLongChau _mediaManagerLongChau;
    private readonly IDataSourceRepository _dataSourceRepository;
    private string BASEURL = string.Empty;
    public SyncArticleLongChauBackgroundWorker(IArticleLongChauRepository articleLongChauRepository, IMediaLongChauRepository mediaLongChauRepository, MediaManagerLongChau mediaManagerLongChau, IDataSourceRepository dataSourceRepository)
    {
        _articleLongChauRepository = articleLongChauRepository;
        _mediaLongChauRepository = mediaLongChauRepository;
        _mediaManagerLongChau = mediaManagerLongChau;
        _dataSourceRepository = dataSourceRepository;

        RecurringJobId            = nameof(SyncArticleLongChauBackgroundWorker);
        CronExpression            = Cron.Daily(0,0);
    }

    public override async Task DoWorkAsync()
    {
        var dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.LongChauUrl));
        if (dataSource == null)
        {
            return;
        }
        BASEURL = dataSource.PostToSite;
        
        var articles = await _articleLongChauRepository.GetListWithNavigationPropertiesAsync();
        foreach (var article in articles)
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
       
        var client =await InitClient();
        var result = await client.Posts.CreateAsync(post);
        if (result != null)
        {
            articleNav.Article.LastSyncedAt = DateTime.UtcNow;
            await _articleLongChauRepository.UpdateAsync(articleNav.Article, true);
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
            FeaturedMedia = featureMedia?.Id
        };
        
        return post;
    }
    
    private string ReplaceImageUrls(string contentHtml, List<MediaItem> medias)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(contentHtml);
        foreach (var node in htmlDoc.DocumentNode.Descendants("//img"))
        {
            var mediaIdAttributeValue = node.Attributes["@media-id"].Value;
            var media = medias.FirstOrDefault(x => mediaIdAttributeValue.Contains(x.Title.Raw));
            
            if (media != null)
            {
                node.SetAttributeValue("src", media.Link );
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
            var medias = await _mediaLongChauRepository.GetListAsync(x => mediaIds.Contains(x.Id));
            //pass the Wordpress REST API base address as string
            var client =await InitClient();
        
            if (medias != null)
            {
                var mediaItems = new List<MediaItem>();
                foreach (var media in medias)
                {
                    var stream = await _mediaManagerLongChau.GetFileStream(media.Name);
                    mediaItems.Add(await client.Media.CreateAsync(stream,media.Name, media.ContentType));
                }

                return mediaItems;
            }

        }
        return null;
    }
    
    private async Task<MediaItem> PostFeatureMediasAsync(Media media)
    {
        if (media != null)
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
        client.Auth.UseBasicAuth("admin", "123456");
        return Task.FromResult(client);
    }
}