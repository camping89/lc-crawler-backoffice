using System.Collections.Generic;
using LC.Crawler.BackOffice.ArticleComments;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.Medias;

namespace LC.Crawler.BackOffice.Articles;

public class ArticleWithNavigationPropertiesResultDto
{
    public string DataSource { get; set; }
    public ArticleResultDto Article { get; set; }
    public List<CategoryResultDto> Categories { get; set; }
    public List<MediaResultDto> Medias { get; set; }
    
    //public List<ArticleCommentResultDto> Comments { get; set; }
}