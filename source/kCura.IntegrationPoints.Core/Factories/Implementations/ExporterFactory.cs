using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ExporterFactory : IExporterFactory
	{
		private readonly IOnBehalfOfUserClaimsPrincipalFactory _claimsPrincipalFactory;
		private readonly IRepositoryFactory _repositoryFactory;

		public ExporterFactory(IOnBehalfOfUserClaimsPrincipalFactory claimsPrincipalFactory, IRepositoryFactory repositoryFactory)
		{
			_claimsPrincipalFactory = claimsPrincipalFactory;
			_repositoryFactory = repositoryFactory;
		}

		public IExporterService BuildExporter(IJobStopManager jobStopManager, FieldMap[] mappedFiles, string config, int savedSearchArtifactId, int onBehalfOfUser)
		{
			if (onBehalfOfUser == 0)
			{
				onBehalfOfUser = 9;
			}
			ClaimsPrincipal claimsPrincipal = _claimsPrincipalFactory.CreateClaimsPrincipal(onBehalfOfUser);
			return new RelativityExporterService(_repositoryFactory, jobStopManager, claimsPrincipal, mappedFiles, 0, config, savedSearchArtifactId);
		}
	}
}