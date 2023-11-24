using kCura.Apps.Common.Utils.Serializers;
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
            ISerializer serializer,
            bool disableNativeLocationValidation,
            bool disableNativeValidation)
          : base(fieldQuery, factory, jobFactory, helper, serializer)
        {
            DisableNativeLocationValidation = disableNativeLocationValidation;
            DisableNativeValidation = disableNativeValidation;
        }
    }
}
