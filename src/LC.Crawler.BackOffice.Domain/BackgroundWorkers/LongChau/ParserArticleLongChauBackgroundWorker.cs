// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net.Http;
// using System.Threading.Tasks;
// using System.Xml.Linq;
// using HtmlAgilityPack;
// using LC.Crawler.BackOffice.Articles;
// using LC.Crawler.BackOffice.Configs;
// using LC.Crawler.BackOffice.Core;
// using LC.Crawler.BackOffice.DataSources;
// using LC.Crawler.BackOffice.Helpers;
// using LC.Crawler.BackOffice.Medias;
// using Volo.Abp.BackgroundWorkers.Hangfire;
//
// namespace LC.Crawler.BackOffice.BackgroundWorkers.LongChau;
//
// public class ParserArticleLongChauBackgroundWorker : HangfireBackgroundWorkerBase
// {
//     private readonly MediaManagerLongChau _mediaManager;
//     private readonly ArticleManager _articleManager;
//     private readonly IArticleLongChauRepository _articleLongChauRepository;
//     private readonly IDataSourceRepository _dataSourceRepository;
//
//     public ParserArticleLongChauBackgroundWorker(MediaManagerLongChau mediaManager, IArticleLongChauRepository articleLongChauRepository, IDataSourceRepository dataSourceRepository, ArticleManager articleManager)
//     {
//         _mediaManager = mediaManager;
//         _articleLongChauRepository = articleLongChauRepository;
//         _dataSourceRepository = dataSourceRepository;
//         _articleManager = articleManager;
//         RecurringJobId            = nameof(ParserArticleLongChauBackgroundWorker);
//         CronExpression            = GlobalCronConsts.Every60Minutes;
//         
//     }
//     
//     public override async Task DoWorkAsync()
//     {
//         var articles = await _articleLongChauRepository.GetListAsync();
//         foreach (var article in articles)
//         {
//             if (!string.IsNullOrEmpty(article.Content))
//             {
//                 var mediaUrls = article.Content.GetImageUrls();
//
//                 if (mediaUrls.Any())
//                 {
//                     var medias = mediaUrls.Select(url => new Media()
//                     {
//                         Url = url,
//                         IsDowloaded = false
//                     }).ToList();
//                     await _mediaManager.CreateManyAsync(medias);
//
//                     article.Content = StringHtmlHelper.ReplaceImageUrls(article.Content, medias);
//                         
//                     foreach (var media in medias)
//                     {
//                         article.AddMedia(media.Id);
//                     }
//
//                     await _articleLongChauRepository.UpdateAsync(article, true);
//                 }
//             }
//         }
//
//     }
//     
//
// }