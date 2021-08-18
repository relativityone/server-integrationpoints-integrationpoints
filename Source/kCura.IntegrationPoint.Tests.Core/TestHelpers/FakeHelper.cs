using System;
using Moq;
using Relativity.API;

namespace kCura.IntegrationPoints.Tests.Core.Helpers
{
    [Serializable]
    public class FakeHelper : IHelper
    {
        [NonSerialized]
        public Mock<IHelper> HelperMock;

        public FakeHelper(Mock<IHelper> helperMock)
        {
            HelperMock = helperMock;
        }

        public void Dispose()
        {
            HelperMock.Object.Dispose();
        }

        public IDBContext GetDBContext(int caseID)
        {
            return HelperMock.Object.GetDBContext(caseID);
        }

        public IServicesMgr GetServicesManager()
        {
            return HelperMock.Object.GetServicesManager();
        }

        public IUrlHelper GetUrlHelper()
        {
            return HelperMock.Object.GetUrlHelper();
        }

        public ILogFactory GetLoggerFactory()
        {
            return HelperMock.Object.GetLoggerFactory();
        }

        public string ResourceDBPrepend()
        {
            return HelperMock.Object.ResourceDBPrepend();
        }

        public string ResourceDBPrepend(IDBContext context)
        {
            return HelperMock.Object.ResourceDBPrepend(context);
        }

        public string GetSchemalessResourceDataBasePrepend(IDBContext context)
        {
            return HelperMock.Object.GetSchemalessResourceDataBasePrepend(context);
        }

        public Guid GetGuid(int workspaceID, int artifactID)
        {
            return HelperMock.Object.GetGuid(workspaceID, artifactID);
        }

        public ISecretStore GetSecretStore()
        {
            return HelperMock.Object.GetSecretStore();
        }

        public IInstanceSettingsBundle GetInstanceSettingBundle()
        {
            return HelperMock.Object.GetInstanceSettingBundle();
        }

        public IStringSanitizer GetStringSanitizer(int workspaceID)
        {
            return HelperMock.Object.GetStringSanitizer(workspaceID);
        }
    }
}
