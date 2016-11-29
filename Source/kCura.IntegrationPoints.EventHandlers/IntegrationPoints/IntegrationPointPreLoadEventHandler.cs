﻿using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Adaptors.Implementations;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Guid("54E65983-C59F-42CA-89CC-9AC30F447619")]
	[Description("This is a details pre load event handler for Integration Point RDO")]
	public class IntegrationPointPreLoadEventHandler : PreLoadEventHandler
	{
		private IIntegrationPointViewPreLoad _integrationPointViewPreLoad;

		public override IIntegrationPointViewPreLoad IntegrationPointViewPreLoad
		{
			get
			{
				return _integrationPointViewPreLoad ??
						(_integrationPointViewPreLoad =
							new IntegrationPointViewPreLoad(ServiceContextFactory.CreateCaseServiceContext(Helper, Application.ArtifactID), 
							new RelativityProviderSourceConfiguration(Helper, new KeplerWorkspaceRepository(new ObjectQueryManagerAdaptor(Helper, -1, (int)ArtifactType.Case))),
							new RelativityProviderDestinationConfiguration(Helper),
								new IntegrationPointFieldsConstants()));
			}
			set { _integrationPointViewPreLoad = value; }
		}
	}
}