using LC.Crawler.BackOffice.MongoDB;
using Xunit;

namespace LC.Crawler.BackOffice;

[CollectionDefinition(BackOfficeTestConsts.CollectionDefinitionName)]
public class BackOfficeDomainCollection : BackOfficeMongoDbCollectionFixtureBase
{

}
