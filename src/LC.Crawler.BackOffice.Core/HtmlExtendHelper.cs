using System.Web;
using static System.String;

namespace LC.Crawler.BackOffice.Core;

public static class HtmlExtendHelper
{
    public static List<string> GetImageUrls(this string contentHtml)
    {
        var htmlDocument =new HtmlAgilityPack.HtmlDocument();
        htmlDocument.LoadHtml(contentHtml);
        var urls = htmlDocument.DocumentNode.Descendants("img")
            .Select(e =>
            {
                var url = string.Empty;
                if (e.Attributes.Any(x=>x.Name == "data-src"))
                {
                    url = e.GetAttributeValue("data-src", null);
                }
                else
                {
                    url =  e.GetAttributeValue("src", null);
                }

                return url;
            })
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
    
    public static string RemoveQueryStringByKey(string url, string key = "")
    {                   
        var uri = new Uri(url);

        // this gets all the query string key value pairs as a collection
        var newQueryString = HttpUtility.ParseQueryString(uri.Query);
        
        // this gets the page path from root without QueryString
        var pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

        if (key == "") return pagePathWithoutQueryString;
        
        // this removes the key if exists
        newQueryString.Remove(key);

        return newQueryString.Count > 0
            ? $"{pagePathWithoutQueryString}?{newQueryString}"
            : pagePathWithoutQueryString;
    }
}