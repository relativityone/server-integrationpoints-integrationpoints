using System;
using Relativity.API;

namespace kCura.IntegrationPoint.Tests.Core
{
    public class EHHelper : IEHHelper
    {
        private readonly IHelper _helper;
        private readonly int _activeCaseArtifactId;

        public EHHelper(IHelper helper, int workspaceArtifactId)
        {
            _helper = helper;
            _activeCaseArtifactId = workspaceArtifactId;
        }

        public void Dispose()
        {
        }

        public IDBContext GetDBContext(int caseID)
        {
            return _helper.GetDBContext(caseID);
        }

        public IServicesMgr GetServicesManager()
        {
            return _helper.GetServicesManager();
        }

        public IUrlHelper GetUrlHelper()
        {
            return _helper.GetUrlHelper();
        }

        public ILogFactory GetLoggerFactory()
        {
            return _helper.GetLoggerFactory();
        }

        public string ResourceDBPrepend()
        {
            return _helper.ResourceDBPrepend();
        }

        public string ResourceDBPrepend(IDBContext context)
        {
            return _helper.ResourceDBPrepend(context);
        }

        public string GetSchemalessResourceDataBasePrepend(IDBContext context)
        {
            return _helper.GetSchemalessResourceDataBasePrepend(context);
        }

        public Guid GetGuid(int workspaceID, int artifactID)
        {
            return _helper.GetGuid(workspaceID, artifactID);
        }

        public ISecretStore GetSecretStore()
        {
            throw new NotImplementedException();
        }

        public IInstanceSettingsBundle GetInstanceSettingBundle()
        {
            throw new NotImplementedException();
        }

        public IStringSanitizer GetStringSanitizer(int workspaceID)
        {
            throw new NotImplementedException();
        }

        public IAuthenticationMgr GetAuthenticationManager()
        {
            throw new NotImplementedException();
        }

        public int GetActiveCaseID()
        {
            return _activeCaseArtifactId;
        }
    }
}