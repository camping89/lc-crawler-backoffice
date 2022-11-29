using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Medias;

namespace LC.Crawler.BackOffice.Helpers;

public static class StringHtmlHelper
{
    public static string SetContentMediaIds(string contentHtml, List<Media> medias)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(contentHtml);
        foreach (var node in htmlDoc.DocumentNode.Descendants("img"))
        {
            if (node.Attributes.Any(x =>x.Name == "data-src"))
            {
                var src = node.Attributes[@"data-src"].Value;
                var media = medias.FirstOrDefault(x => x.Url.Contains(src));
            
                if (media != null)
                {
                    node.Attributes.Add("@media-id", $"media/{media.Id}");
                    node.SetAttributeValue("src", string.Empty);
                }
            }
            else
            {
                var src = node.Attributes[@"src"].Value;
                var media = medias.FirstOrDefault(x => x.Url.Contains(src));
            
                if (media != null)
                {
                    node.Attributes.Add("@media-id", $"media/{media.Id}");
                    node.SetAttributeValue("src", string.Empty);
                }
            }
        }

        var newHtml = htmlDoc.DocumentNode.WriteTo();
        return newHtml;
    }
    
    public static string ReplaceImageUrls(string contentHtml, List<Media> medias)
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

    public static bool CompareUrls(string firstUrl, string secondUrl)
    {
        try
        {
            var uri1 = new Uri(firstUrl);
            var uri2 = new Uri(secondUrl);

            return Uri.Compare(uri1, uri2, UriComponents.Host | UriComponents.PathAndQuery, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) is 0;
        }
        catch (Exception e)
        {
            // ignored
        }

        return firstUrl.Equals(secondUrl, StringComparison.InvariantCultureIgnoreCase);
    }
}