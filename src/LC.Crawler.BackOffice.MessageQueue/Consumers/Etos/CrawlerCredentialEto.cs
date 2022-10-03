using LC.Crawler.BackOffice.Enums;

namespace LC.Crawler.BackOffice.MessageQueue.Consumers.Etos;

public class CrawlerCredentialEto
{
    public CrawlerCredentialEto(CredentialEto crawlerCredential, AccountEto crawlerAccount, ProxyEto crawlerProxy)
    {
        CrawlerCredential = crawlerCredential;
        CrawlerAccount = crawlerAccount;
        CrawlerProxy = crawlerProxy;
    }
    
    public CredentialEto CrawlerCredential { get; set; }

        
    public AccountEto CrawlerAccount { get; set; }
        
        
    public ProxyEto CrawlerProxy { get; set; }
}

public class CredentialEto
{
    public Guid Id { get; set; }
    public virtual DataSourceType DataSourceType { get; set; }
    public Guid? CrawlerAccountId { get; set; }
    public Guid? CrawlerProxyId { get; set; }
        
    public DateTime? CrawledAt { get; set; }
    public bool IsAvailable { get; set; }
    public string ConcurrencyStamp { get; set; }
}

public class AccountEto
{
    public Guid Id { get; set; }
    public virtual string Username { get; set; }

    
    public virtual string Password { get; set; }

    
    public virtual string TwoFactorCode { get; set; }

    public virtual AccountType AccountType { get; set; }

    public virtual AccountStatus AccountStatus { get; set; }

    
    public virtual string Email { get; set; }

    
    public virtual string EmailPassword { get; set; }

    public virtual bool IsActive { get; set; }
    public List<CookieEto> Cookies { get; set; }

    public string ConcurrencyStamp { get; set; }
}

public class CookieEto
{
    public string Domain { get; set; }
    public float? Expires { get; set; }
    public bool HttpOnly { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public bool Secure { get; set; }
    public string Value { get; set; }
}

public class ProxyEto
{
    public Guid Id { get; set; }
    public virtual string Ip { get; set; }

    public virtual int Port { get; set; }

    public virtual string Protocol { get; set; }

    public virtual string Username { get; set; }

    public virtual string Password { get; set; }

    public virtual DateTime? PingedAt { get; set; }

    public virtual bool IsActive { get; set; }

    public string ConcurrencyStamp { get; set; }
}