using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
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
		  kCura.IntegrationPoints.Contracts.ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
		  IJobHistoryService jobHistoryService,
		  JobHistoryErrorService jobHistoryErrorService,
		  IJobManager jobManager) : base(caseServiceContext,
		   helper,
		   dataProviderFactory,
		   serializer,
		   appDomainRdoSynchronizerFactoryFactory,
		   jobHistoryService,
		   jobHistoryErrorService,
		   jobManager)
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
