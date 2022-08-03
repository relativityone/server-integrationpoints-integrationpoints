using kCura.Apps.Common.Data;
using Moq;
using Relativity.API;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
    public class FakeSqlServiceFactory : ISqlServiceFactory
    {
        public IDBContext GetSqlService()
        {
            return new Mock<IDBContext>().Object;
        }
    }
}
