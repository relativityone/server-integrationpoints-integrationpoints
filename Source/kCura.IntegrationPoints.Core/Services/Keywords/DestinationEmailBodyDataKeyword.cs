﻿using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
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
            IIntegrationPointService integrationPointService,
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
                integrationPointService,
                diagnosticLog)
        {
            _job = job;
        }

        public string Convert()
        {
            SetIntegrationPoint(_job);
            List<FieldMap> fieldMap = IntegrationPointDto.FieldMappings;
            fieldMap.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);
            List<FieldEntry> destinationFields = GetDestinationFields(fieldMap.ToArray());
            IDataSynchronizer destinationProvider = GetDestinationProvider(base.DestinationProvider, IntegrationPointDto.DestinationConfiguration, _job);

            string returnValue = string.Empty;
            if (destinationProvider is IEmailBodyData)
            {
                string importSettingsString = Serializer.Serialize(IntegrationPointDto.DestinationConfiguration);
                returnValue = ((IEmailBodyData)destinationProvider).GetEmailBodyData(destinationFields, importSettingsString);
            }

            return returnValue;
        }
    }
}
