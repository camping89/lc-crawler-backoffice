using static System.String;

namespace LC.Crawler.BackOffice.Core;

public static class HtmlExtendHelper
{
    public static List<string> GetImageUrls(this string contentHtml)
    {
        var htmlDocument =new HtmlAgilityPack.HtmlDocument();
        htmlDocument.LoadHtml(contentHtml);
        var urls = htmlDocument.DocumentNode.Descendants("img")
            .Select(e => e.GetAttributeValue("src", null))
            .Where(s => !IsNullOrEmpty(s)).ToList();
        return urls;
    }
    
    public static List<string> GetVideoUrls(this string contentHtml)
    {
        var htmlDocument =new HtmlAgilityPack.HtmlDocument();
        htmlDocument.LoadHtml(contentHtml);
        var urls = htmlDocument.DocumentNode.Descendants("video")
            .Select(e => e.GetAttributeValue("src", null))
            .Where(s => !IsNullOrEmpty(s)).ToList();
        return urls;
    }
}