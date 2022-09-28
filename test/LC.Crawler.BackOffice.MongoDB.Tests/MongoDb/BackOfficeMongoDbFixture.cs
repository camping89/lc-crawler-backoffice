using System;
using Mongo2Go;

namespace LC.Crawler.BackOffice.MongoDB;

public class BackOfficeMongoDbFixture : IDisposable
{
    private static readonly MongoDbRunner MongoDbRunner;
    public static readonly string ConnectionString;

    static BackOfficeMongoDbFixture()
    {
        MongoDbRunner = MongoDbRunner.Start(singleNodeReplSet: true, singleNodeReplSetWaitTimeout: 20);
        ConnectionString = MongoDbRunner.ConnectionString;
    }

    public void Dispose()
    {
        MongoDbRunner?.Dispose();
    }
}
