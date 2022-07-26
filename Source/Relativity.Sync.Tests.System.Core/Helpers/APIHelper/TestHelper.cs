using System;
using Moq;
using Relativity.API;
using Relativity.Sync.Tests.System.Core.Stubs;

namespace Relativity.Sync.Tests.System.Core.Helpers.APIHelper
{
    public class TestHelper : IHelper
    {
        public void Dispose()
        {
        }

        public IDBContext GetDBContext(int caseID)
        {
            return new TestDbContext(caseID);
        }

        public ILogFactory GetLoggerFactory()
        {
            var logFactory = new Mock<ILogFactory>();
            logFactory.Setup(x => x.GetLogger()).Returns(TestLogHelper.GetLogger());

            return logFactory.Object;
        }

        public IServicesMgr GetServicesManager()
        {
            return new ServicesManagerStub();
        }

        public IInstanceSettingsBundle GetInstanceSettingBundle()
        {
            var instanceSettingBundle = new Mock<IInstanceSettingsBundle>();
            return instanceSettingBundle.Object;
        }

        #region Not Implemented

        public IUrlHelper GetUrlHelper()
        {
            throw new NotImplementedException();
        }

        public string ResourceDBPrepend()
        {
            throw new NotImplementedException();
        }

        public string ResourceDBPrepend(IDBContext context)
        {
            throw new NotImplementedException();
        }

        public string GetSchemalessResourceDataBasePrepend(IDBContext context)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int workspaceID, int artifactID)
        {
            throw new NotImplementedException();
        }

        public ISecretStore GetSecretStore()
        {
            throw new NotImplementedException();
        }

        public IStringSanitizer GetStringSanitizer(int workspaceID)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
