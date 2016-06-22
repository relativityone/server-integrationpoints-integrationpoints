using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;

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

		public IExporterService BuildExporter(FieldMap[] mappedFiles, string config, int savedSearchArtifactId, int onBehalfOfUser)
		{
			if (onBehalfOfUser == 0)
			{
				onBehalfOfUser = 9;
			}
			ClaimsPrincipal claimsPrincipal = _claimsPrincipalFactory.CreateClaimsPrincipal(onBehalfOfUser);
			return new RelativityExporterService(_repositoryFactory, claimsPrincipal, mappedFiles, 0, config, savedSearchArtifactId);
		}
	}
}