namespace LC.Crawler.BackOffice.Enums;

public enum AccountStatus
{
    Unknown = 0,
    New = 10,
    Ready = 11,
    Active = 20,
    Inactive = 21,
    LoginApprovalNeeded = 30,
    BlockedTemporary = 31,
    Banned = 99
}