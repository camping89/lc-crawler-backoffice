using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.CrawlerProxies
{
    public class CrawlerProxyCreateDto
    {
        [Required]
        public string Ip { get; set; }
        public string Port { get; set; }
        [Required]
        public string Protocol { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime? PingedAt { get; set; }
        public bool IsActive { get; set; }
    }
}