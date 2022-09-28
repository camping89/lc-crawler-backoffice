using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace LC.Crawler.BackOffice.Data;

/* This is used if database provider does't define
 * IBackOfficeDbSchemaMigrator implementation.
 */
public class NullBackOfficeDbSchemaMigrator : IBackOfficeDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
