using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Configs;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.BackgroundWorkers.Hangfire;
using WordPressPCL;
using WordPressPCL.Models;

namespace LC.Crawler.BackOffice.BackgroundWorkers.LongChau;

public class SyncArticleLongChauBackgroundWorker : HangfireBackgroundWorkerBase
{
    private readonly IArticleLongChauRepository _articleLongChauRepository;
    private readonly IArticleRepository _articleRepository;
    private readonly IMediaLongChauRepository _mediaLongChauRepository;
    private readonly MediaManagerLongChau _mediaManagerLongChau;

    private readonly string BASEURL = "https://ecom1.cakiengmini.com";

    public SyncArticleLongChauBackgroundWorker(IArticleLongChauRepository articleLongChauRepository, IMediaLongChauRepository mediaLongChauRepository, MediaManagerLongChau mediaManagerLongChau, IArticleRepository articleRepository)
    {
        _articleLongChauRepository = articleLongChauRepository;
        _mediaLongChauRepository = mediaLongChauRepository;
        _mediaManagerLongChau = mediaManagerLongChau;
        _articleRepository = articleRepository;

        RecurringJobId            = nameof(SyncArticleLongChauBackgroundWorker);
        CronExpression            = Cron.Daily(0,0);
    }

    public override async Task DoWorkAsync()
    {
        var articles = await _articleRepository.GetListWithNavigationPropertiesAsync();
        foreach (var article in articles)
        {
            await PostToWPAsync(article);
        }
        
    }

    private async Task PostToWPAsync(ArticleWithNavigationProperties articleNav)
    {
        var featureMedia = await PostFeatureMediasAsync( articleNav);
        var postMedias = await PostMediasAsync(articleNav);
        
        var post = ConvertToPost(articleNav, featureMedia);
        
        //pass the Wordpress REST API base address as string
       
        var client = new WordPressClient($"{BASEURL}/wp-json/");
        client.Auth.UseBasicAuth("admin", "fTu6 yTGB hkd1 ftcq Ggj1 DpQt");
        var result = await client.Posts.CreateAsync(post);
    }

    private Post ConvertToPost(ArticleWithNavigationProperties articleNav,MediaItem featureMedia)
    {
        var article = articleNav.Article;
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
            
        };
        return post;
    }

    private async Task<List<MediaItem>> PostMediasAsync(ArticleWithNavigationProperties articleNav)
    {
        
        //pass the Wordpress REST API base address as string
        var client =await InitClient();

        if (articleNav.Medias != null)
        {
            var mediaItems = new List<MediaItem>();
            foreach (var media in articleNav.Medias)
            {
                var stream = await _mediaManagerLongChau.GetFileStream(media.Name);
                mediaItems.Add(await client.Media.CreateAsync(stream,media.Name, media.ContentType));
            }

            return mediaItems;
        }

        return null;
    }
    
    private async Task<MediaItem> PostFeatureMediasAsync(ArticleWithNavigationProperties articleNav)
    {
        //pass the Wordpress REST API base address as string
        var client = await InitClient();

        if (articleNav.Media != null)
        {
            var media = articleNav.Media;
            var stream = await _mediaManagerLongChau.GetFileStream(media.Name);
            return await client.Media.CreateAsync(stream, media.Name, media.ContentType);
        }

        return null;
    }

    private async Task<WordPressClient> InitClient()
    {
        //pass the Wordpress REST API base address as string
        var client = new WordPressClient($"{BASEURL}/wp-json/");
        client.Auth.UseBasicAuth("admin", "fTu6 yTGB hkd1 ftcq Ggj1 DpQt");
        // client.Auth.UseBearerAuth(JWTPlugin.JWTAuthByEnriqueChavez);
        // await client.Auth.RequestJWTokenAsync("admin", "XvarE5Mt1fbhKCaJRm5lGhYe");
        //var isValidToken = await client.IsValidJWTokenAsync();

        return client;
    }
}