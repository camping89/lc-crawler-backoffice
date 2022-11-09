using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.Domain.Services;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using RestSharp;
using RestSharp.Authenticators;
using Svg;
using Volo.Abp.Auditing;
using WordPressPCL;
using WordPressPCL.Models;
using WordpresCategory = WordPressPCL.Models.Category;
using WordpresTag = WordPressPCL.Models.Tag;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerBase : DomainService
{
    private readonly IAuditingManager _auditingManager;

    public WordpressManagerBase(IAuditingManager auditingManager)
    {
        _auditingManager = auditingManager;
    }

    public async Task DoUpdatePosts(DataSource dataSource)
    {
        var client = await InitClient(dataSource);
        var wpPosts = (await client.Posts.GetAllAsync(useAuth: true)).Where(x => x.Content.Rendered.Contains("href"))
            .ToList();

        Console.WriteLine($"Total: {wpPosts.Count()}");
        var index = 1;
        foreach (var wpPost in wpPosts)
        {
            try
            {
                if (wpPost.Content != null)
                {
                    wpPost.Content.Raw = wpPost.Content.Rendered.RemoveHrefFromA();
                    await client.Posts.UpdateAsync(wpPost);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                continue;
            }

            Console.WriteLine($"Index: {index}");
            index++;
        }
    }

    public async Task<Post> DoSyncPostAsync(DataSource dataSource, ArticleWithNavigationProperties articleNav)
    {
        var client = await InitClient(dataSource);
        var featureMedia = await PostMediaAsync(dataSource, articleNav.Media);
        var contentMedias = await PostMediasAsync(dataSource, articleNav);

        var article = articleNav.Article;
        article.Content = ReplaceImageUrls(article.Content, articleNav.Medias);
        article.Content = ReplaceVideos(article.Content);

        var post = new Post
        {
            Title = new Title(article.Title),
            Content = new Content(article.Content),
            Date = article.CreatedAt,
            Excerpt = new Excerpt(article.Excerpt),
            Status = Status.Pending,
            LiveblogLikes = article.LikeCount,
            CommentStatus = OpenStatus.Open,
            FeaturedMedia = featureMedia?.Id,
            Categories = new List<int>(),
            Tags = new List<int>()
        };

        // categories
        await AddPostCategories(articleNav, client, post, dataSource.Url);

        // tags
        await AddPostTags(client, article, post, dataSource.Url);

        var result = await client.Posts.CreateAsync(post);
        return result;
    }

    private async Task AddPostTags(WordPressClient client, Article article, Post post, string homeUrl)
    {
        using var auditingScope = _auditingManager.BeginScope();

        try
        {
            var wooTags = (await client.Tags.GetAllAsync(useAuth: true)).ToList();
            if (article.Tags.IsNotNullOrEmpty())
            {
                foreach (var tag in article.Tags)
                {
                    var wpTag = wooTags.FirstOrDefault(_ => _.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
                    if (wpTag is null)
                    {
                        wpTag = await client.Tags.CreateAsync(new WordpresTag { Name = tag });
                        wooTags.Add(wpTag);
                    }

                    post.Tags.Add(wpTag.Id);
                }
            }
        }
        catch (Exception ex)
        {
            LogException(_auditingManager.Current.Log, ex, article, homeUrl, "PostTags");
        }
        finally
        {
            //Always save the log
            await auditingScope.SaveAsync();
        }
    }

    private async Task AddPostCategories(ArticleWithNavigationProperties articleNav, WordPressClient client, Post post,
        string homeUrl)
    {
        using var auditingScope = _auditingManager.BeginScope();
        try
        {
            if (articleNav.Categories.IsNotNullOrEmpty())
            {
                post.Categories = new List<int>();
                foreach (var category in articleNav.Categories)
                {
                    var wooCategories = (await client.Categories.GetAllAsync(useAuth: true)).ToList();

                    var encodeName = category.Name.Split("->").LastOrDefault()?.Replace("&", "&amp;").Trim();

                    var wpCategory = wooCategories.FirstOrDefault(x =>
                        encodeName != null && x.Name.Contains(encodeName, StringComparison.InvariantCultureIgnoreCase));
                    if (encodeName is not null)
                    {
                        var wpCategoriesFilter = wooCategories.Where(x =>
                            encodeName.IsNotNullOrEmpty() &&
                            x.Name.Contains(encodeName, StringComparison.InvariantCultureIgnoreCase)).ToList();
                        foreach (var wpCate in wpCategoriesFilter)
                        {
                            var parentCate = wooCategories.FirstOrDefault(x => x.Id == wpCate.Parent);
                            if (parentCate != null && category.Name.Contains(parentCate.Name))
                            {
                                var rootParent = wooCategories.FirstOrDefault(x => x.Id == parentCate.Parent);
                                if ((rootParent != null && category.Name.Contains(rootParent.Name)) ||
                                    parentCate.Parent == 0)
                                {
                                    wpCategory = wpCate;
                                }
                            }
                        }
                    }

                    if (wpCategory is not null)
                    {
                        post.Categories.Add(wpCategory.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogException(_auditingManager.Current.Log, ex, articleNav.Article, homeUrl, "PostCategories");
        }
        finally
        {
            // Always save the log
            await auditingScope.SaveAsync();
        }
    }

    private string ReplaceVideos(string contentHtml)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(contentHtml);
        var divVideos = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class,'VCSortableInPreviewMode')]");
        if (divVideos is null) return contentHtml;

        foreach (var divVideo in divVideos)
        {
            var dataVideo = divVideo.Attributes["data-vid"];
            if (dataVideo is not null)
            {
                var linkVideo = dataVideo.Value;
                if (linkVideo.IsNotNullOrEmpty())
                {
                    divVideo.InnerHtml = $"[video width='1280' height='720' mp4='{linkVideo}']";
                }
            }
        }

        var newHtml = htmlDoc.DocumentNode.WriteTo();
        return newHtml;
    }

    public string ReplaceImageUrls(string contentHtml, List<Media> medias)
    {
        if (!contentHtml.IsNotNullOrEmpty()) return null;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(contentHtml);
        foreach (var node in htmlDoc.DocumentNode.Descendants("img"))
        {
            var mediaIdAttributeValue = node.Attributes["@media-id"].Value;
            var media = medias.FirstOrDefault(x => mediaIdAttributeValue.Contains(x.Id.ToString()));

            if (media != null)
            {
                node.SetAttributeValue("src", media.ExternalUrl);
            }
        }

        var newHtml = htmlDoc.DocumentNode.WriteTo();
        return newHtml;
    }

    private async Task<List<MediaItem>> PostMediasAsync(DataSource dataSource,
        ArticleWithNavigationProperties articleNav)
    {
        if (articleNav is { Medias: { } })
        {
            if (articleNav.Medias != null)
            {
                var mediaItems = new List<MediaItem>();
                foreach (var media in articleNav.Medias.Where(media => media.Url.IsNotNullOrEmpty()))
                {
                    var mediaResult = await PostMediaAsync(dataSource, media);
                    if (mediaResult is null) continue;
                    mediaItems.Add(mediaResult);
                }

                return mediaItems;
            }
        }

        return null;
    }

    public async Task DoSyncCategoriesAsync(DataSource dataSource, List<string> categories)
    {
        var client = await InitClient(dataSource);
        var wooCategories = (await client.Categories.GetAllAsync(useAuth: true)).ToList();

        foreach (var cateStr in categories)
        {
            if (AbpStringExtensions.IsNullOrEmpty(cateStr))
            {
                continue;
            }

            var categoriesTerms = cateStr.Split("->").ToList();
            var cateName = categoriesTerms.FirstOrDefault()?.Trim().Replace("&", "&amp;");
            var wooRootCategory =
                wooCategories.FirstOrDefault(x =>
                    x.Name.Equals(cateName, StringComparison.InvariantCultureIgnoreCase) && x.Parent == 0);
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
                        var wooSubCategory = wooCategories.FirstOrDefault(x =>
                            x.Name.Equals(subCateName, StringComparison.InvariantCultureIgnoreCase) &&
                            x.Parent == cateParent.Id);
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

    private async Task<MediaItem> PostMediaAsync(DataSource dataSource, Media media)
    {
        MediaItem mediaResult = null;

        try
        {
            if (media is null || !media.Url.IsNotNullOrEmpty()) return null;

            var client = await InitClient(dataSource);
            //var stream = await _mediaManagerLongChau.GetFileStream(media.Name);
            if (media.Url.Contains("http") == false)
            {
                media.Url = $"{dataSource.Url}{media.Url}";
            }

            media.Url = HtmlExtendHelper.RemoveQueryStringByKey(media.Url);
            var fileExtension = Path.GetExtension(media.Url);
            if (!fileExtension.IsNotNullOrEmpty()) return null;

            if (fileExtension is FileExtendHelper.SvgExtend)
            {
                var svgContent = await FileExtendHelper.DownloadSvgFile(media.Url);
                if (!svgContent.IsNotNullOrEmpty()) return null;

                var fileName = $"{media.Id}{FileExtendHelper.PngExtend}";
                var svgDoc = SvgDocument.FromSvg<SvgDocument>(svgContent);
                var bitmap = svgDoc.Draw();
                using var stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                mediaResult = await client.Media.CreateAsync(stream, fileName, media.ContentType);
            }
            else
            {
                var fileBytes = await FileExtendHelper.DownloadFile(media.Url);
                if (fileBytes is null) return null;

                using var stream = new MemoryStream(fileBytes);
                var fileName = $"{media.Id}{fileExtension}";

                mediaResult = await client.Media.CreateAsync(stream, fileName, media.ContentType);
            }

            media.ExternalId = mediaResult.Id.ToString();
            media.ExternalUrl = mediaResult.SourceUrl;
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return mediaResult;
    }

    private Task<WordPressClient> InitClient(DataSource dataSource)
    {
        //pass the Wordpress REST API base address as string
        var client = new WordPressClient($"{dataSource.PostToSite}/wp-json/");
        client.Auth.UseBasicAuth(dataSource.Configuration.Username, dataSource.Configuration.Password);
        return Task.FromResult(client);
    }

    public void LogException(AuditLogInfo currentLog, Exception ex, Article article, string url,
        string entity = "Article")
    {
        //Add exceptions
        currentLog.Url = url;
        currentLog.Exceptions.Add(ex);
        if (ex.InnerException is not null)
        {
            currentLog.Exceptions.Add(ex.InnerException);
        }

        currentLog.Comments.Add($"Id: {article.Id}, DataSourceId {article.DataSourceId}");
        currentLog.Comments.Add(ex.StackTrace);
        currentLog.ExtraProperties.Add("C_Entity", entity);
        currentLog.ExtraProperties.Add("C_Message", ex.Message);
        currentLog.ExtraProperties.Add("C_StackTrace", ex.StackTrace);
        currentLog.ExtraProperties.Add("C_Source", ex.Source);
        currentLog.ExtraProperties.Add("C_ExToString", ex.ToString());
    }

    public async Task DoUpdatePostAsync(DataSource dataSource, ArticleWithNavigationProperties articleNav, Post post)
    {
        var client = await InitClient(dataSource);
        post.Content.Raw = ReplaceImageUrls(articleNav.Article.Content, articleNav.Medias);
        await AddPostCategories(articleNav, client, post, dataSource.Url);
        await client.Posts.UpdateAsync(post);
    }
}