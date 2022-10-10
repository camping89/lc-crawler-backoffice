namespace Veek.DataProvider.Crawler.Client.Entities.LC;

public class CrawlArticlePayload
{
    public string Title { get; set; }
    public string ShortDescription { get; set; }
    public string Url { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Category { get; set; }
    public string Content { get; set; }
    public List<string> Tags { get; set; }
    public string FeatureImage { get; set; }
}