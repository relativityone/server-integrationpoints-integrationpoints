using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Internals;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
    public class DestinationEmailBodyDataKeyword : IntegrationPointTaskBase, IKeyword
    {
        private readonly Job _job;

        public string KeywordName => "\\[RIP.DESTINATIONEMAILBODYDATA]";

        public DestinationEmailBodyDataKeyword(
            Job job,
            ICaseServiceContext caseServiceContext,
            IHelper helper,
            IDataProviderFactory dataProviderFactory,
            Apps.Common.Utils.Serializers.ISerializer serializer,
            ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
            IJobHistoryService jobHistoryService,
            IJobHistoryErrorService jobHistoryErrorService,
            IJobManager jobManager,
            IManagerFactory managerFactory,
            IJobService jobService,
            IIntegrationPointRepository integrationPointRepository,
            IDiagnosticLog diagnosticLog)
            : base(
                caseServiceContext,
                helper,
                dataProviderFactory,
                serializer,
                appDomainRdoSynchronizerFactoryFactory,
                jobHistoryService,
                jobHistoryErrorService,
                jobManager,
                managerFactory,
                jobService,
                integrationPointRepository,
                diagnosticLog)
        {
            _job = job;
        }

        public string Convert()
        {
            SetIntegrationPoint(_job);
            string destinationConfiguration = this.IntegrationPoint.DestinationConfiguration;
            IEnumerable<FieldMap> fieldMap = GetFieldMap(this.IntegrationPoint.FieldMappings);
            FieldMap[] fieldMaps = fieldMap as FieldMap[] ?? fieldMap.ToArray();
            List<FieldEntry> destinationFields = GetDestinationFields(fieldMaps);
            IDataSynchronizer destinationProvider = GetDestinationProvider(base.DestinationProvider, destinationConfiguration, _job);

            string returnValue = string.Empty;
            if (destinationProvider is IEmailBodyData)
            {
                returnValue = ((IEmailBodyData)destinationProvider).GetEmailBodyData(destinationFields, destinationConfiguration);
            }

            return returnValue;
        }
    }
}
