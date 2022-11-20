namespace LC.Crawler.BackOffice.Enums;

public enum PageSyncStatus
{
    Unknown    = 0,
    Pending    = 10,
    InProgress = 20,
    Completed  = 30
}
public enum PageSyncStatusType
{
    SyncArticle,
    ResyncArticle,
    SyncProduct,
    ResyncProduct
}