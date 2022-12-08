using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Extensions;
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
                var nodeDataAttr = node.Attributes[@"data-src"];
                if (nodeDataAttr is not null)
                {
                    var src   = nodeDataAttr.Value;
                    var media = medias.FirstOrDefault(x => x.Url.Contains(src));
            
                    if (media != null)
                    {
                        node.Attributes.Add("@media-id", $"media/{media.Id}");
                        node.SetAttributeValue("src", string.Empty);
                    }
                }
            }
            else
            {
                var nodeSrcAttr = node.Attributes[@"src"];
                if (nodeSrcAttr is not null)
                {
                    var src = nodeSrcAttr?.Value;
                    if (src.IsNotNullOrEmpty())
                    {
                        var media = medias.FirstOrDefault(x => x.Url.Contains(src));
                        if (media != null)
                        {
                            node.Attributes.Add("@media-id", $"media/{media.Id}");
                            node.SetAttributeValue("src", string.Empty);
                        }
                    }
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
            var nodeMediaAttr         = node.Attributes["@media-id"];
            if (nodeMediaAttr is not null)
            {
                var mediaIdAttributeValue = nodeMediaAttr.Value;
                var media                 = medias.FirstOrDefault(x => mediaIdAttributeValue.Contains(x.Id.ToString()));
                if (media != null)
                {
                    node.SetAttributeValue("src", media.ExternalUrl);
                }
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