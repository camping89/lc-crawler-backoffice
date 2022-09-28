using System.Threading.Tasks;

namespace LC.Crawler.BackOffice.Data;

public interface IBackOfficeDbSchemaMigrator
{
    Task MigrateAsync();
}
