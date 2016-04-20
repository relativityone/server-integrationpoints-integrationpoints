using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Data.Contexts;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ExporterFactory : IExporterFactory
	{
		private readonly IOnBehalfOfUserClaimsPrincipleFactory _claimsPrincipleFactory;

		public ExporterFactory(IOnBehalfOfUserClaimsPrincipleFactory claimsPrincipleFactory)
		{
			_claimsPrincipleFactory = claimsPrincipleFactory;
		}

		public IExporterService BuildExporter(FieldMap[] mappedFiles, string config, int onBehalfOfUser)
		{
			if (onBehalfOfUser == 0)
			{
				onBehalfOfUser = 9;
			}

			ClaimsPrincipal claimsPrincipal = _claimsPrincipleFactory.CreateClaimsPrinciple(onBehalfOfUser);
			return new RelativityExporterService(claimsPrincipal, mappedFiles, 0, config);
		}
	}
}