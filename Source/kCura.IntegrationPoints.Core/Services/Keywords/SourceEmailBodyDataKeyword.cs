using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
	public class SourceEmailBodyDataKeyword : IntegrationPointTaskBase, IKeyword
	{
		public string KeywordName { get { return "\\[RIP.SOURCEEMAILBODYDATA]"; } }

		private readonly Job _job;
		public SourceEmailBodyDataKeyword(Job job,
		  ICaseServiceContext caseServiceContext,
		  IHelper helper,
		  IDataProviderFactory dataProviderFactory,
		  kCura.Apps.Common.Utils.Serializers.ISerializer serializer,
		  ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
		  IJobHistoryService jobHistoryService,
		  IJobHistoryErrorService jobHistoryErrorService,
		  IJobManager jobManager,
		  IManagerFactory managerFactory,
		  IContextContainerFactory contextContainerFactory,
		  IJobService jobService,
		  IIntegrationPointRepository integrationPointRepository) : base(caseServiceContext,
		   helper,
		   dataProviderFactory,
		   serializer,
		   appDomainRdoSynchronizerFactoryFactory,
		   jobHistoryService,
		   jobHistoryErrorService,
		   jobManager,
		   managerFactory,
		   contextContainerFactory,
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
