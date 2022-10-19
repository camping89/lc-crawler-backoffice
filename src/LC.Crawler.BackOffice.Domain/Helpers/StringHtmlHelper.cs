using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using LC.Crawler.BackOffice.Medias;

namespace LC.Crawler.BackOffice.Helpers;

public static class StringHtmlHelper
{
    public static string ReplaceImageUrls(string contentHtml, List<Media> medias)
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
}