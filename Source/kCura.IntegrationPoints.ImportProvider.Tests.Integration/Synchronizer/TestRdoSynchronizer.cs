using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Relativity.API;

namespace kCura.IntegrationPoints.ImportProvider.Tests.Integration
{
    /// <summary>
    /// TestRdoSynchronizer class is to allow some protected properties to be set for test
    /// </summary>
    public class TestRdoSynchronizer : RdoSynchronizer
    {
        public TestRdoSynchronizer(
            IRelativityFieldQuery fieldQuery,
            IImportApiFactory factory,
            IImportJobFactory jobFactory,
            IHelper helper,
            IDiagnosticLog diagnosticLog,
            string webApiPath,
            bool disableNativeLocationValidation,
            bool disableNativeValidation,
            IInstanceSettingsManager instanceSettingsManager)
          : base(fieldQuery, factory, jobFactory, helper, diagnosticLog, instanceSettingsManager)
        {
            WebAPIPath = webApiPath;
            DisableNativeLocationValidation = disableNativeLocationValidation;
            DisableNativeValidation = disableNativeValidation;
        }
    }
}
