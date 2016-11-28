using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Guid("F10B8D7A-AB22-4A27-999B-88E8C8A9BFB5")]
	[Description("This is a details pre load event handler for Integration Point Profile RDO")]
	public class IntegrationPointProfilePreLoadEventHandler : PreLoadEventHandler
	{
		private IIntegrationPointViewPreLoad _integrationPointViewPreLoad;

		public override IIntegrationPointViewPreLoad IntegrationPointViewPreLoad
		{
			get
			{
				return _integrationPointViewPreLoad ??
						(_integrationPointViewPreLoad =
							new IntegrationPointViewPreLoad(ServiceContextFactory.CreateCaseServiceContext(Helper, Application.ArtifactID), 
							new RelativityProviderSourceConfiguration(Helper),
							new RelativityProviderDestinationConfiguration(Helper),
								new IntegrationPointProfileFieldsConstants()));
			}
			set { _integrationPointViewPreLoad = value; }
		}
	}
}