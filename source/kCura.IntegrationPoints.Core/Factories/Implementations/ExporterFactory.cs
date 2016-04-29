using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Services.Exporter;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Factories;
using Relativity.Core;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ExporterFactory : IExporterFactory
	{
		private readonly IOnBehalfOfUserClaimsPrincipleFactory _claimsPrincipleFactory;
		private readonly IRepositoryFactory _repositoryFactory;

		public ExporterFactory(IOnBehalfOfUserClaimsPrincipleFactory claimsPrincipleFactory, IRepositoryFactory repositoryFactory)
		{
			_claimsPrincipleFactory = claimsPrincipleFactory;
			_repositoryFactory = repositoryFactory;
		}

		public IExporterService BuildExporter(FieldMap[] mappedFiles, string config, int onBehalfOfUser)
		{
			if (onBehalfOfUser == 0)
			{
				onBehalfOfUser = 9;
			}
			ClaimsPrincipal claimsPrincipal = _claimsPrincipleFactory.CreateClaimsPrinciple(onBehalfOfUser);
			return new RelativityExporterService(_repositoryFactory, claimsPrincipal, mappedFiles, 0, config);
		}
	}
}