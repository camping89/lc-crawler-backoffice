using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using WordPressPCL;
using WordPressPCL.Models;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerSieuThiSongKhoe : DomainService
{
    private readonly IArticleSieuThiSongKhoeRepository _articleSieuThiSongKhoeRepository;
    private readonly IMediaSieuThiSongKhoeRepository   _mediaSieuThiSongKhoeRepository;
    private readonly MediaManagerSieuThiSongKhoe       _mediaManagerSieuThiSongKhoe;
    private readonly IDataSourceRepository             _dataSourceRepository;
    private          string                            BASEURL = string.Empty;
    private          DataSource                        _dataSource;

    public WordpressManagerSieuThiSongKhoe(IDataSourceRepository dataSourceRepository, MediaManagerSieuThiSongKhoe mediaManagerSieuThiSongKhoe, IMediaSieuThiSongKhoeRepository mediaSieuThiSongKhoeRepository, IArticleSieuThiSongKhoeRepository articleSieuThiSongKhoeRepository)
    {
        _dataSourceRepository             = dataSourceRepository;
        _mediaManagerSieuThiSongKhoe      = mediaManagerSieuThiSongKhoe;
        _mediaSieuThiSongKhoeRepository   = mediaSieuThiSongKhoeRepository;
        _articleSieuThiSongKhoeRepository = articleSieuThiSongKhoeRepository;
    }

    public async Task DoSyncToWordpress()
    {
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.SieuThiSongKhoeUrl));
        if (_dataSource == null)
        {
            return;
        }
        BASEURL = _dataSource.PostToSite;
        
        var articles = await _articleSieuThiSongKhoeRepository.GetListWithNavigationPropertiesAsync();
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
            await _articleSieuThiSongKhoeRepository.UpdateAsync(articleNav.Article, true);
        }
    }

    private Post ConvertToPost(ArticleWithNavigationProperties articleNav, MediaItem featureMedia, List<MediaItem> contentMedias)
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
            var medias = await _mediaSieuThiSongKhoeRepository.GetListAsync(x => mediaIds.Contains(x.Id));
            //pass the Wordpress REST API base address as string
            var client =await InitClient();
        
            if (medias != null)
            {
                var mediaItems = new List<MediaItem>();
                foreach (var media in medias)
                {
                    var stream = await _mediaManagerSieuThiSongKhoe.GetFileStream(media.Name);
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

            var stream = await _mediaManagerSieuThiSongKhoe.GetFileStream(media.Name);
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