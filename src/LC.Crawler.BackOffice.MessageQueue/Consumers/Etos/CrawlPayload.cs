using LC.Crawler.BackOffice.MessageQueue.Consumers.LC;

namespace LC.Crawler.BackOffice.MessageQueue.Consumers.Etos
{
    public class CrawlPayload
    {
        public CrawlPayload()
        {
            HashTags = new List<string>();
            Urls = new List<string>();
        }
        public string Url { get; set; }

        /// <summary>
        /// This is used for affiliate urls (usually called short links)
        /// </summary>
        public List<string> Urls { get; set; }

        public string Content { get; set; }
        public List<string> Images { get; set; }
        public List<string> HashTags { get; set; }
        
        public string AvatarUrl { get; set; }
        
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string CreateFuid { get; set; }
        
        /// <summary>
        /// No need to crawl, just copied from API
        /// </summary>
        public bool IsNotAvailable { get; set; }
        
        // TODO Vu.Nguyen: later crawl this
        public string FacebookId { get; set; }

        public string HashTagsString
        {
            get
            {
                if (HashTags.Any())
                {
                    return string.Join("\r\n", HashTags);
                }

                return string.Empty;
            }
        }

        public string UrlsString
        {
            get
            {
                if (Urls.Any())
                {
                    return string.Join("\r\n", Urls);
                }

                return string.Empty;
            }
        }


        #region LC

        public CrawlEcommercePayload CrawlEcommercePayload { get; set; }

        #endregion
    }
}