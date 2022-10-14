using System;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.Payloads;

public class CrawlArticlePayload
{
    public CrawlArticlePayload()
    {
        ArticlesPayload = new List<ArticlePayload>();
    }
    
    public string Url { get; set; }

    public List<ArticlePayload> ArticlesPayload { get; set; }
}

public class ArticlePayload
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