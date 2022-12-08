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
using IdentityServer4.Extensions;
using LC.Crawler.BackOffice.Extensions;
using Svg;
using Volo.Abp.Auditing;
using WordPressPCL;
using WordPressPCL.Models;
using WordPressPCL.Utility;
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
    
    public async Task DoUpdatePostTags(List<Tag> wpTags, List<string> tags, Post post, WordPressClient client)
    {
        if (tags.IsNotNullOrEmpty())
        {
            var tagIds = new List<int>();
            foreach (var tag in tags)
            {
                var wpTag = wpTags.FirstOrDefault(_ => _.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
                if (wpTag is null)
                {
                    wpTag = await client.Tags.CreateAsync(new WordpresTag { Name = tag });
                    wpTags.Add(wpTag);
                }

                tagIds.Add(wpTag.Id);
            }

            var deleteTags = post.Tags.Where(_ => !tagIds.Contains(_)).ToList();
            if (deleteTags.IsNotNullOrEmpty())
            {
                post.Tags.RemoveAll(_ => deleteTags.Contains(_));
            }

            foreach (var tagId in tagIds)
            {
                if (post.Tags.All(_ => _ != tagId))
                {
                    post.Tags.AddRange(tagIds);
                }
            }
            
            await client.Posts.UpdateAsync(post);
        }
    }

    public async Task CleanDuplicatePostsAsync(DataSource dataSource)
    {
        var client = await InitClient(dataSource);
        var posts = new List<Post>();
        var pageIndex = 1;
        while (true)
        {
            //var route = "posts".SetQueryParam("status", "pending").SetQueryParam("per_page", "100").SetQueryParam("page", pageIndex.ToString());
            var resultPosts = await client.Posts.QueryAsync(new PostsQueryBuilder()
            {
                Statuses = new List<Status>()
                {
                    Status.Pending,
                    Status.Publish
                },
                Page = pageIndex,
                PerPage = 100
            },true);

            posts.AddRange(resultPosts);
            Console.WriteLine($"Page {pageIndex}");
            
            if (resultPosts.IsNullOrEmpty() || resultPosts.Count() < 100)
            {
                break;
            }

            pageIndex++;
        }

        foreach (var g in posts.GroupBy(x=>x.Title.Rendered))
        {
            var count = g.Count();
            if (count > 1)
            {
                foreach (var post in g.Take(count - 1))
                {
                    await client.Posts.DeleteAsync(post.Id);
                    Console.WriteLine($"Delete post {post.Id}");
                }
            }
        }
    }
    public async Task<Post> DoSyncPostAsync(DataSource dataSource, ArticleWithNavigationProperties articleNav, List<Tag> wooTags, MediaItem featureMedia)
    {
        var client = await InitClient(dataSource);

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
        await AddPostTags(client, article, post, dataSource.Url, wooTags);


        var result = await client.Posts.CreateAsync(post);
        return result;
    }

    private async Task AddPostTags(WordPressClient client, Article article, Post post, string homeUrl, List<Tag> wpTags)
    {
        using var auditingScope = _auditingManager.BeginScope();

        try
        {
            //var wooTags = (await client.Tags.GetAllAsync(useAuth: true)).ToList();
            if (article.Tags.IsNotNullOrEmpty())
            {
                foreach (var tag in article.Tags)
                {
                    var wpTag = wpTags.FirstOrDefault(_ => _.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
                    if (wpTag is null)
                    {
                        wpTag = await client.Tags.CreateAsync(new WordpresTag { Name = tag });
                        wpTags.Add(wpTag);
                    }

                    post.Tags.Add(wpTag.Id);
                }
            }
        }
        catch (Exception ex)
        {
            LogException(_auditingManager.Current.Log, ex, $"{article.Id}", homeUrl, "PostTags");
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
                    category.Name = category.Name.Replace("&", "&amp;");
                    
                    var wooCategories = (await client.Categories.GetAllAsync(useAuth: true)).ToList();

                    var categoriesTerms = category.Name.Split("->").Select(x=>x.Trim()).ToList();

                    var encodeName = categoriesTerms.LastOrDefault()?.Trim();

                    var wpCategory = wooCategories.FirstOrDefault(x =>
                        encodeName != null && x.Name.Equals(encodeName, StringComparison.InvariantCultureIgnoreCase) && x.Parent == 0);
                    if (encodeName is not null)
                    {
                        if (categoriesTerms.Count > 1)
                        {
                            var wpCategoriesFilter = wooCategories.Where(x =>
                                encodeName.IsNotNullOrEmpty() &&
                                x.Name.Equals(encodeName, StringComparison.InvariantCultureIgnoreCase)).ToList();
                            foreach (var wpCate in wpCategoriesFilter)
                            {
                                var parentCate = wooCategories.FirstOrDefault(x => x.Id == wpCate.Parent);
                                if (parentCate != null && category.Name.Contains(parentCate.Name, StringComparison.InvariantCultureIgnoreCase))
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
            LogException(_auditingManager.Current.Log, ex, $"{articleNav.Article.Id}", homeUrl, "PostCategories");
        }
        finally
        {
            // Always save the log
            await auditingScope.SaveAsync();
        }
    }

    private string ReplaceVideos(string contentHtml)
    {
        if (!contentHtml.IsNotNullOrEmpty()) return string.Empty;
        
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
        if (!contentHtml.IsNotNullOrEmpty()) return string.Empty;

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(contentHtml);
        foreach (var node in htmlDoc.DocumentNode.Descendants("img"))
        {
            var mediaIdAttributeValue = node.Attributes["@media-id"]?.Value;
            if (!mediaIdAttributeValue.IsNotNullOrEmpty())
            {
                continue;
            }
            var media = medias.FirstOrDefault(x => mediaIdAttributeValue.Contains(x.Id.ToString()));

            if (media != null)
            {
                node.SetAttributeValue("src", media.ExternalUrl);
            }
        }

        var newHtml = htmlDoc.DocumentNode.WriteTo();
        return newHtml;
    }

    public async Task<List<MediaItem>> PostMediasAsync(DataSource dataSource,
        ArticleWithNavigationProperties articleNav)
    {
        if (articleNav is { Medias: { } })
        {
            if (articleNav.Medias != null)
            {
                var mediaItems = new List<MediaItem>();
                foreach (var media in articleNav.Medias.Where(media => string.IsNullOrEmpty(media.ExternalUrl) && media.Url.IsNotNullOrEmpty()))
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

            var handleCateStr = cateStr.Replace("&", "&amp;").Trim();
            var categoriesTerms = handleCateStr.Split("->").ToList();
            var cateName = categoriesTerms.FirstOrDefault()?.Trim();
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
                        var subCateName = categoriesTerms[i].Trim();
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

    public async Task<MediaItem> PostMediaAsync(DataSource dataSource, Media media)
    {
        MediaItem mediaResult = null;
        using var auditingScope = _auditingManager.BeginScope();
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
                var stream = await FileExtendHelper.DownloadFileStream(media.Url);
                var fileName = $"{media.Id}{fileExtension}";

                if (stream is not null)
                {
                    mediaResult = await client.Media.CreateAsync(stream, fileName, media.ContentType);
                }
            }

            if (mediaResult is not null)
            {
                media.ExternalId  = mediaResult.Id.ToString();
                media.ExternalUrl = mediaResult.SourceUrl;
            }
        }
        catch (Exception e)
        {
            LogImageException(_auditingManager.Current.Log, e, media?.Url, "PostImage");
            Console.WriteLine(e);
        }
        finally
        {
            await auditingScope.SaveAsync();
        }

        return mediaResult;
    }

    public Task<WordPressClient> InitClient(DataSource dataSource)
    {
        //pass the Wordpress REST API base address as string
        var client = new WordPressClient($"{dataSource.PostToSite}/wp-json/");
        client.Auth.UseBasicAuth(dataSource.Configuration.Username, dataSource.Configuration.Password);
        return Task.FromResult(client);
    }

    public void LogException(AuditLogInfo currentLog, Exception ex, string articleId, string url,
        string entity = "Article")
    {
        //Add exceptions
        currentLog.Url = url;
        currentLog.Exceptions.Add(ex);
        if (ex.InnerException is not null)
        {
            currentLog.Exceptions.Add(ex.InnerException);
        }

        currentLog.Comments.Add(ex.StackTrace);
        currentLog.Comments.Add($"Id: {articleId}");
        currentLog.ExtraProperties.Add("C_Entity", entity);
        currentLog.ExtraProperties.Add("C_Message", ex.Message);
        currentLog.ExtraProperties.Add("C_StackTrace", ex.StackTrace);
        currentLog.ExtraProperties.Add("C_Source", ex.Source);
        currentLog.ExtraProperties.Add("C_ExToString", ex.ToString());
    }
    
    public void LogImageException(AuditLogInfo currentLog, Exception ex, string imageUrl,
        string entity = "Article Image")
    {
        //Add exceptions
        currentLog.Url = imageUrl;
        currentLog.Exceptions.Add(ex);
        if (ex.InnerException is not null)
        {
            currentLog.Exceptions.Add(ex.InnerException);
        }
        
        currentLog.Comments.Add(ex.StackTrace);
        currentLog.ExtraProperties.Add("C_Entity", entity);
        currentLog.ExtraProperties.Add("C_Message", ex.Message);
        currentLog.ExtraProperties.Add("C_StackTrace", ex.StackTrace);
        currentLog.ExtraProperties.Add("C_Source", ex.Source);
        currentLog.ExtraProperties.Add("C_ExToString", ex.ToString());
    }

    public async Task DoUpdatePostAsync(DataSource dataSource, ArticleWithNavigationProperties articleNav, Post post, MediaItem featureMedia)
    {
        var client = await InitClient(dataSource);
        post.Content.Raw = ReplaceImageUrls(articleNav.Article.Content, articleNav.Medias);
        if (featureMedia is not null)
        {
            post.FeaturedMedia = featureMedia.Id;
        }
        await AddPostCategories(articleNav, client, post, dataSource.Url);
        await client.Posts.UpdateAsync(post);
    }

    public async Task<List<Tag>> GetAllTags(DataSource dataSource)
    {
        Console.WriteLine($"Get Tags");
        var client = await InitClient(dataSource);
        return (await client.Tags.GetAllAsync(useAuth: true)).ToList();
    }

    public async Task<List<Post>> GetAllPosts(DataSource dataSource, WordPressClient client = null)
    {
        if (client is null)
        {
            client = await InitClient(dataSource);
        }
        
        var posts = new List<Post>();
        var pageIndex = 1;
            
        while (true)
        {
            var wpPosts = new List<Post>();
            try
            {
                var resultPosts = await client.Posts.QueryAsync(new PostsQueryBuilder()
                {
                    Statuses = new List<Status>()
                    {
                        Status.Pending,
                        Status.Publish
                    },
                    Page    = pageIndex,
                    PerPage = 100,
                
                },true);
                wpPosts.AddRange(resultPosts);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            posts.AddRange(wpPosts);
            Console.WriteLine($"Page {pageIndex}");
                
            if (wpPosts.IsNullOrEmpty() || wpPosts.Count() < 100)
            {
                break;
            }

            pageIndex++;
        }

        return posts;
    }
    
    public async Task UpdatePostDetails(DataSource dataSource,Post post, Article article, List<Media> medias, WordPressClient client)
    {
        post.Title.Raw   = article.Title;
        post.Excerpt.Raw = article.Excerpt;
        
        if (medias.IsNotNullOrEmpty())
        {
            article.Content  = ReplaceImageUrls(article.Content, medias);
            article.Content  = ReplaceVideos(article.Content);
        }
        
        post.Content.Raw = article.Content;

        await client.Posts.UpdateAsync(post);
    }
}