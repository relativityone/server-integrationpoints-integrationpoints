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
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Internals;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
	public class SourceEmailBodyDataKeyword : IntegrationPointTaskBase, IKeyword
	{
		private readonly Job _job;

		public string KeywordName => "\\[RIP.SOURCEEMAILBODYDATA]";

		public SourceEmailBodyDataKeyword(Job job,
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
			IIntegrationPointRepository integrationPointRepository)
			: base(caseServiceContext,
				helper,
				dataProviderFactory,
				serializer,
				appDomainRdoSynchronizerFactoryFactory,
				jobHistoryService,
				jobHistoryErrorService,
				jobManager,
				managerFactory,
				jobService,
				integrationPointRepository)
		{
			_job = job;
		}

		public string Convert()
		{
			SetIntegrationPoint(_job);
			string sourceConfiguration = IntegrationPoint.SourceConfiguration;
			IEnumerable<FieldMap> fieldMap = GetFieldMap(IntegrationPoint.FieldMappings);
			FieldMap[] fieldMaps = fieldMap as FieldMap[] ?? fieldMap.ToArray();
			List<FieldEntry> sourceFields = GetSourceFields(fieldMaps);
			IDataSourceProvider sourceProvider = GetSourceProvider(SourceProvider, _job);

			string returnValue = string.Empty;
			if (sourceProvider is IEmailBodyData)
			{
				returnValue = ((IEmailBodyData)sourceProvider).GetEmailBodyData(sourceFields, sourceConfiguration);
			}

			return returnValue;
		}
	}
}
