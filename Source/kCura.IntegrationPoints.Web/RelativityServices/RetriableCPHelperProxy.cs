using System;
using kCura.IntegrationPoints.Web.RelativityServices.Exceptions;
using Polly;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.RelativityServices
{
    [Serializable]
    internal class RetriableCPHelperProxy : ICPHelper
    {
        [NonSerialized]
        private IAPILog _logger; // logger implementation is non serializable

        private const int _RETRY_LIMIT = 2;
        private readonly ICPHelper _baseCpHelper;

        private IAPILog Logger =>
            _logger
            ?? (_logger = GetLoggerFactory().GetLogger().ForContext<RetriableCPHelperProxy>());

        public RetriableCPHelperProxy(ICPHelper baseCpHelper)
        {
            if (baseCpHelper is RetriableCPHelperProxy)
            {
                throw new BaseCpHelperCannotBeTypeOfRetriableCpHelperException();
            }

            _baseCpHelper = baseCpHelper;
        }

        public int GetActiveCaseID()
        {
            return Retry(_baseCpHelper.GetActiveCaseID, onResult: 0);
        }

        public IDBContext GetDBContext(int caseId)
        {
            return _baseCpHelper.GetDBContext(caseId);
        }

        public IServicesMgr GetServicesManager()
        {
            return _baseCpHelper.GetServicesManager();
        }

        public IUrlHelper GetUrlHelper()
        {
            return _baseCpHelper.GetUrlHelper();
        }

        public ILogFactory GetLoggerFactory()
        {
            return _baseCpHelper.GetLoggerFactory();
        }

        public string ResourceDBPrepend()
        {
            return _baseCpHelper.ResourceDBPrepend();
        }

        public string ResourceDBPrepend(IDBContext context)
        {
            return _baseCpHelper.ResourceDBPrepend(context);
        }

        public string GetSchemalessResourceDataBasePrepend(IDBContext context)
        {
            return _baseCpHelper.GetSchemalessResourceDataBasePrepend(context);
        }

        public Guid GetGuid(int workspaceId, int artifactId)
        {
            return _baseCpHelper.GetGuid(workspaceId, artifactId);
        }

        public ISecretStore GetSecretStore()
        {
            return _baseCpHelper.GetSecretStore();
        }

        public IInstanceSettingsBundle GetInstanceSettingBundle()
        {
            return _baseCpHelper.GetInstanceSettingBundle();
        }

        public IStringSanitizer GetStringSanitizer(int workspaceId)
        {
            return _baseCpHelper.GetStringSanitizer(workspaceId);
        }

        public IAuthenticationMgr GetAuthenticationManager()
        {
            return _baseCpHelper.GetAuthenticationManager();
        }

        public ICSRFManager GetCSRFManager()
        {
            return _baseCpHelper.GetCSRFManager();
        }

        public void Dispose()
        {
            _baseCpHelper.Dispose();
        }

        private T Retry<T>(Func<T> func, T onResult)
        {
            return Policy
                .HandleResult<T>(result => result.Equals(onResult))
                .Retry(_RETRY_LIMIT, (result, retryCount) =>
                    Logger.LogError("Error while calling {0} with result: {1}. Retrying {1}...",
                        func.Method.Name,
                        result.Result,
                        retryCount))
                .Execute(func);
        }
    }
}