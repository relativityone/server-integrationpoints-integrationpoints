using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
	public class DestinationEmailBodyDataKeyword : IntegrationPointTaskBase, IKeyword
	{
		public string KeywordName { get { return "\\[RIP.DESTINATIONEMAILBODYDATA]"; } }

		private readonly Job _job;
		public DestinationEmailBodyDataKeyword(Job job,
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
		  IJobService jobService) : base(caseServiceContext,
		   helper,
		   dataProviderFactory,
		   serializer,
		   appDomainRdoSynchronizerFactoryFactory,
		   jobHistoryService,
		   jobHistoryErrorService,
		   jobManager,
		   managerFactory,
		   contextContainerFactory,
		   jobService)
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
