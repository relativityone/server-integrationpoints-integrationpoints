using System;
using kCura.Apps.Common.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Web
{
    public class HelperConfigSqlServiceFactoryWrapper : ISqlServiceFactory
    {
        private readonly Lazy<HelperConfigSqlServiceFactory> _helperConfigSqlServiceFactory;
        public HelperConfigSqlServiceFactoryWrapper(Func<IHelper> helperFactory)
        {
            _helperConfigSqlServiceFactory = new Lazy<HelperConfigSqlServiceFactory>(() => new HelperConfigSqlServiceFactory(helperFactory()));
        }
        public IDBContext GetSqlService()
        {
            return _helperConfigSqlServiceFactory.Value.GetSqlService();
        }
    }
}