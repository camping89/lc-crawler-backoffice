using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LC.Crawler.BackOffice.Medias
{
    public class MediaCreateDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string ContentType { get; set; }
        [Required]
        public string Url { get; set; }
        public string Description { get; set; }
        public bool IsDowloaded { get; set; }
    }
}