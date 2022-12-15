using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LC.Crawler.BackOffice.Articles;
using LC.Crawler.BackOffice.Categories;
using LC.Crawler.BackOffice.DataSources;
using LC.Crawler.BackOffice.Enums;
using LC.Crawler.BackOffice.Extensions;
using LC.Crawler.BackOffice.Medias;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using WooCategory = WordPressPCL.Models.Category;

namespace LC.Crawler.BackOffice.Wordpress;

public class WordpressManagerSongKhoeMedplus : DomainService
{
    private readonly IArticleSongKhoeMedplusRepository  _articleSongKhoeMedplusRepository;
    private readonly ICategorySongKhoeMedplusRepository _categorySongKhoeMedplusRepository;
    private readonly IMediaSongKhoeMedplusRepository    _mediaSongKhoeMedplusRepository;
    private readonly IDataSourceRepository              _dataSourceRepository;
    private          DataSource                         _dataSource;
    private readonly WordpressManagerBase               _wordpressManagerBase;
    private readonly IAuditingManager                   _auditingManager;
    private readonly DataSourceManager _dataSourceManager;

    public WordpressManagerSongKhoeMedplus(IArticleSongKhoeMedplusRepository  articleSongKhoeMedplusRepository, 
                                           ICategorySongKhoeMedplusRepository categorySongKhoeMedplusRepository, 
                                           IMediaSongKhoeMedplusRepository    mediaSongKhoeMedplusRepository, 
                                           IDataSourceRepository              dataSourceRepository,
                                           WordpressManagerBase               wordpressManagerBase,
                                           IAuditingManager                   auditingManager,
                                           DataSourceManager dataSourceManager)
    {
        _articleSongKhoeMedplusRepository  = articleSongKhoeMedplusRepository;
        _categorySongKhoeMedplusRepository = categorySongKhoeMedplusRepository;
        _mediaSongKhoeMedplusRepository    = mediaSongKhoeMedplusRepository;
        _dataSourceRepository              = dataSourceRepository;
        _wordpressManagerBase              = wordpressManagerBase;
        _auditingManager                   = auditingManager;
        _dataSourceManager = dataSourceManager;
    }

    public async Task DoSyncPostAsync()
    {
        // get datasource
        Console.WriteLine($"Start Sync: {PageDataSourceConsts.SongKhoeMedplusUrl}");
        _dataSource = await _dataSourceRepository.FirstOrDefaultAsync(x => x.Url.Contains(PageDataSourceConsts.SongKhoeMedplusUrl));
        if (_dataSource is not { ShouldReSyncArticle: true })
        {
            return;
        }
        
        // // update re-sync status
        
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncArticle, PageSyncStatus.InProgress);
        
        //TODO Remove after clean data
        // get categories
        var categories = (await _categorySongKhoeMedplusRepository.GetListAsync()).Where(_ => 
            !_.Name.Contains("Thuốc A-Z", StringComparison.InvariantCultureIgnoreCase) 
            && !_.Name.Contains("Nuôi dạy con -> Kỹ năng nuôi con ->", StringComparison.InvariantCultureIgnoreCase) 
            && !_.Name.Equals("Dinh dưỡng thai kỳ", StringComparison.InvariantCultureIgnoreCase) 
            && !_.Name.Equals("Sức khỏe -> Bệnh A-Z -> Nhi khoa", StringComparison.InvariantCultureIgnoreCase)).ToList();
        var categoryIds = categories.Select(_ => _.Id).ToList();
        
        // get article ids
        var limitDate = new DateTime(2018, 01, 01); 
        var articleIds = (await _articleSongKhoeMedplusRepository.GetQueryableAsync())
                        .Where(x => x.DataSourceId == _dataSource.Id && x.Content != null 
                                                                     && x.LastSyncedAt == null 
                                                                     && x.CreatedAt >= limitDate)
                        .Select(x=>x.Id).ToList();

        // get all tags
        var wpTags = await _wordpressManagerBase.GetAllTags(_dataSource);
        
        var count = 0;
        var total = articleIds.Count;
        // sync article to wp
        foreach (var articleId in articleIds)
        {
            using var auditingScope = _auditingManager.BeginScope();
            
            try
            {
                count++;
                Console.WriteLine($"Progressing: {count}/{total}");
                var articleNav = await _articleSongKhoeMedplusRepository.GetWithNavigationPropertiesAsync(articleId);
                if (articleNav.Categories.Any(_ => !_.Id.IsIn(categoryIds))) continue;
                
                var featureMedia = await _wordpressManagerBase.PostMediaAsync(_dataSource, articleNav.Media);
                await _wordpressManagerBase.PostMediasAsync(_dataSource, articleNav);
                
                if (articleNav.Media is not null)
                {
                    await _mediaSongKhoeMedplusRepository.UpdateAsync(articleNav.Media, true);
                }

                if (articleNav.Medias.IsNotNullOrEmpty())
                {
                    await _mediaSongKhoeMedplusRepository.UpdateManyAsync(articleNav.Medias, true);
                }

                var post = await _wordpressManagerBase.DoSyncPostAsync(_dataSource, articleNav, wpTags, featureMedia);
                if (post is not null) 
                {
                    var article = await _articleSongKhoeMedplusRepository.GetAsync(articleId);
                    article.ExternalId   = post.Id.To<int>();
                    article.LastSyncedAt = DateTime.UtcNow;
                    await _articleSongKhoeMedplusRepository.UpdateAsync(article, true);
                    await CheckFormatEntity(article);
                }
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, $"{articleId}", PageDataSourceConsts.SongKhoeMedplusUrl, "DoSyncPostAsync");
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }
        
        // // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.SyncArticle, PageSyncStatus.Completed);
    }
    
    public async Task DoReSyncPostAsync()
    {
        // get data source
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SongKhoeMedplusUrl));
        if (_dataSource is not { ShouldReSyncArticle: true })
        {
            return;
        }
        
        // // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.ResyncArticle, PageSyncStatus.InProgress);
        
        // get all posts
        var client   = await _wordpressManagerBase.InitClient(_dataSource);
        var allPosts = await _wordpressManagerBase.GetAllPosts(_dataSource, client);

        // sync articles from wp
        foreach (var post in allPosts)
        {
            using var auditingScope = _auditingManager.BeginScope();
            
            try
            {
                var article = await _articleSongKhoeMedplusRepository.FirstOrDefaultAsync(x => x.ExternalId == post.Id.To<int>()) 
                           ?? await _articleSongKhoeMedplusRepository.FirstOrDefaultAsync(x => x.Title.Equals(post.Title.Rendered));
                if (article is not null)
                {
                    var mediaIds = article.Medias?.Select(x => x.MediaId).ToList();
                    var medias   = await _mediaSongKhoeMedplusRepository.GetListAsync(_ => mediaIds.Contains(_.Id));
                    foreach (var media in medias)
                    {
                        await _wordpressManagerBase.PostMediaAsync(_dataSource, media);
                    }
                    await _mediaSongKhoeMedplusRepository.UpdateManyAsync(medias);

                    await _wordpressManagerBase.UpdatePostDetails(_dataSource,post, article, medias, client);
                    
                    article.LastSyncedAt =   DateTime.UtcNow;
                    article.ExternalId   ??= post.Id.To<int>();
                    await _articleSongKhoeMedplusRepository.UpdateAsync(article, true);
                    await CheckFormatEntity(article, "resync");
                }   
            }
            catch (Exception ex)
            {
                //Add exceptions
                _wordpressManagerBase.LogException(_auditingManager.Current.Log, ex, null, PageDataSourceConsts.SongKhoeMedplusUrl, "DoReSyncPostAsync");
            }
            finally
            {
                //Always save the log
                await auditingScope.SaveAsync();
            }
        }
        
        // // update re-sync status
        await _dataSourceManager.DoUpdateSyncStatus(_dataSource.Id, PageSyncStatusType.ResyncArticle, PageSyncStatus.Completed);
    }
    
    public async Task DoSyncCategoriesAsync()
    {
        _dataSource = await _dataSourceRepository.GetAsync(x => x.Url.Contains(PageDataSourceConsts.SongKhoeMedplusUrl));
        if (_dataSource == null)
        {
            return;
        }
        var categories = (await _categorySongKhoeMedplusRepository.GetListAsync(x => x.CategoryType == CategoryType.Article)).Where(x => 
                !x.Name.Contains("Thuốc A-Z", StringComparison.InvariantCultureIgnoreCase) 
                && !x.Name.Contains("Nuôi dạy con -> Kỹ năng nuôi con ->", StringComparison.InvariantCultureIgnoreCase)
                && !x.Name.Equals("Dinh dưỡng thai kỳ", StringComparison.InvariantCultureIgnoreCase) 
                && !x.Name.Equals("Sức khỏe -> Bệnh A-Z -> Nhi khoa", StringComparison.InvariantCultureIgnoreCase))
                        .Select(x => x.Name).Distinct().ToList();
        // Category
        await _wordpressManagerBase.DoSyncCategoriesAsync(_dataSource, categories);
    }
    
    private async Task CheckFormatEntity(Article articleEntity, string type = "sync")
    {
        try
        {
            var checkArticle = await _articleSongKhoeMedplusRepository.GetAsync(articleEntity.Id);
        }
        catch (Exception e)
        {
            var date = DateTime.UtcNow;
            var lines = new List<string>()
            {
                $"Exception: {e.Message}",
                $"Article Id: {articleEntity.Id}"
            };
            var logFileName = $"C:\\Work\\ErrorLogs\\Sites\\rror-records_{type}_songkhoemedplus_{date:dd-MM-yyyy_hh-mm}.txt";
            await File.WriteAllLinesAsync(logFileName, lines);
            throw;
        }
    }
}