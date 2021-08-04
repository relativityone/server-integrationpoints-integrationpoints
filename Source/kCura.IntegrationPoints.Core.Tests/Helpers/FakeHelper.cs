using System;
using Moq;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
    [Serializable]
    public class FakeHelper : IHelper
    {
        [NonSerialized]
        public Mock<IHelper> _helperMock;

        public FakeHelper(Mock<IHelper> helperMock)
        {
            _helperMock = helperMock;
        }

        public void Dispose()
        {
            _helperMock.Object.Dispose();
        }

        public IDBContext GetDBContext(int caseID)
        {
            return _helperMock.Object.GetDBContext(caseID);
        }

        public IServicesMgr GetServicesManager()
        {
            return _helperMock.Object.GetServicesManager();
        }

        public IUrlHelper GetUrlHelper()
        {
            return _helperMock.Object.GetUrlHelper();
        }

        public ILogFactory GetLoggerFactory()
        {
            return _helperMock.Object.GetLoggerFactory();
        }

        public string ResourceDBPrepend()
        {
            return _helperMock.Object.ResourceDBPrepend();
        }

        public string ResourceDBPrepend(IDBContext context)
        {
            return _helperMock.Object.ResourceDBPrepend(context);
        }

        public string GetSchemalessResourceDataBasePrepend(IDBContext context)
        {
            return _helperMock.Object.GetSchemalessResourceDataBasePrepend(context);
        }

        public Guid GetGuid(int workspaceID, int artifactID)
        {
            return _helperMock.Object.GetGuid(workspaceID, artifactID);
        }

        public ISecretStore GetSecretStore()
        {
            return _helperMock.Object.GetSecretStore();
        }

        public IInstanceSettingsBundle GetInstanceSettingBundle()
        {
            return _helperMock.Object.GetInstanceSettingBundle();
        }

        public IStringSanitizer GetStringSanitizer(int workspaceID)
        {
            return _helperMock.Object.GetStringSanitizer(workspaceID);
        }
    }
}
