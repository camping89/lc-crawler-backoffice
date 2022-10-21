using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.Domain.Services;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Core;
using LC.Crawler.BackOffice.Extensions;
using Svg;
using WordPressPCL;
using WordPressPCL.Models;
using WordpresCategory = WordPressPCL.Models.Category;
using WordpresTag = WordPressPCL.Models.Tag;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerBase : DomainService
{
    public async Task<Post> DoSyncPostAsync(DataSource dataSource, ArticleWithNavigationProperties articleNav)
    {
        var client = await InitClient(dataSource);
        var featureMedia = await PostMediaAsync(dataSource, articleNav.Media);
        var contentMedias = await PostMediasAsync(dataSource, articleNav);

        var article = articleNav.Article;
        article.Content = ReplaceImageUrls(article.Content, articleNav.Medias);

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
        var wooCategories = (await client.Categories.GetAllAsync(useAuth: true)).ToList();

        var encodeName = articleNav.Categories.FirstOrDefault()?.Name.Split("->").LastOrDefault()?.Replace("&", "&amp;").Trim();
        var wpCate = wooCategories.FirstOrDefault(x =>
            encodeName.IsNotNullOrEmpty() && x.Name.Contains(encodeName, StringComparison.InvariantCultureIgnoreCase));
        if (wpCate != null)
        {
            post.Categories.Add(wpCate.Id);
        }
        
        // tags
        var wooTags = (await client.Tags.GetAllAsync(useAuth: true)).ToList();
        if (article.Tags.Any())
        {
            foreach (var tag in article.Tags)
            {
                var wpTag = wooTags.FirstOrDefault(_ => _.Name == tag);
                if (wpTag is null)
                {
                    wpTag = await client.Tags.CreateAsync(new WordpresTag { Name = tag });
                    wooTags.Add(wpTag);
                }
                
                post.Tags.Add(wpTag.Id);
            }
        }

        var result = await client.Posts.CreateAsync(post);
        return result;
    }

    private string ReplaceImageUrls(string contentHtml, List<Media> medias)
    {
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
                wooCategories.FirstOrDefault(x => x.Name.Equals(cateName, StringComparison.InvariantCultureIgnoreCase));
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
                            x.Name.Equals(subCateName, StringComparison.InvariantCultureIgnoreCase));
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
        MediaItem mediaResult;

        if (media is null || !media.Url.IsNotNullOrEmpty()) return null;

        var client = await InitClient(dataSource);
        //var stream = await _mediaManagerLongChau.GetFileStream(media.Name);
        if (media.Url.Contains("http") == false)
        {
            media.Url = $"{dataSource.Url}{media.Url}";
        }

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

        return mediaResult;
    }

    private Task<WordPressClient> InitClient(DataSource dataSource)
    {
        //pass the Wordpress REST API base address as string
        var client = new WordPressClient($"{dataSource.PostToSite}/wp-json/");
        client.Auth.UseBasicAuth(dataSource.Configuration.Username, dataSource.Configuration.Password);
        return Task.FromResult(client);
    }
}