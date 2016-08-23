using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Interfaces.Private.Extensions;

namespace kCura.IntegrationPoints.Services
{
	public class IntegrationPointManager : KeplerServiceBase, IIntegrationPointManager
	{
		public async Task<IntegrationPointModel> CreateIntegrationPointAsync(CreateIntegrationPointRequest request)
		{
			return await Task.Run(async () =>
			{
				using (IWindsorContainer container = await GetDependenciesContainerAsync(request.WorkspaceArtifactId).ConfigureAwait(false))
				{
					request.ValidateRequest();
					request.ValidatePermission(container);

					IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();

					IntegrationModel ip = request.CreateIntegrationPointModel();
					IntegrationPointModel returnVal = new IntegrationPointModel
					{
						ArtifactId = integrationPointService.SaveIntegration(ip),
						Name = request.Name,
						SourceProvider = request.SourceProviderArtifactId
					};
					return returnVal;
				}
			}).ConfigureAwait(false);
		}

		public async Task<IntegrationPointModel> UpdateIntegrationPointAsync(UpdateIntegrationPointRequest request)
		{
			return await Task.Run(async () =>
			{
				using (IWindsorContainer container =
						await GetDependenciesContainerAsync(request.WorkspaceArtifactId).ConfigureAwait(false))
				{
					request.ValidateRequest();
					request.ValidatePermission(container);

					IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
					try
					{
						integrationPointService.GetRdo(request.IntegrationPointArtifactId);
					}
					catch (Exception)
					{
						throw new Exception($"The requested object with artifact id {request.IntegrationPointArtifactId} does not exist.");
					}

					IntegrationModel ip = request.CreateIntegrationPointModel();
					IntegrationPointModel returnVal = new IntegrationPointModel
					{
						ArtifactId = integrationPointService.SaveIntegration(ip),
						Name = request.Name,
						SourceProvider = request.SourceProviderArtifactId
					};

					return returnVal;
				}
			}).ConfigureAwait(false);
		}

		public async Task<IntegrationPointModel> GetIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId)
		{
			return await Task.Run(async () =>
			{
				using (IWindsorContainer container = await GetDependenciesContainerAsync(workspaceArtifactId).ConfigureAwait(false))
				{
					IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
					Data.IntegrationPoint integrationPoint = integrationPointService.GetRdo(integrationPointArtifactId);
					try
					{
						integrationPointService.GetRdo(integrationPointArtifactId);
					}
					catch (Exception)
					{
						throw new Exception($"The requested object with artifact id {integrationPointArtifactId} does not exist.");
					}

					var result = integrationPoint.ToIntegrationPointModel();
					return result;
				}
			}).ConfigureAwait(false);
		}

		public async Task RunIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId)
		{
			using (IWindsorContainer container = await GetDependenciesContainerAsync(workspaceArtifactId).ConfigureAwait(false))
			{
				IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
				int userId = global::Relativity.API.Services.Helper.GetAuthenticationManager().UserInfo.ArtifactID;
				integrationPointService.RunIntegrationPoint(workspaceArtifactId, integrationPointArtifactId, userId);
			}
		}

		public async Task<IList<IntegrationPointModel>> GetAllIntegrationPointsAsync(int workspaceArtifactId)
		{
			return await Task.Run(async () =>
			{
				using (IWindsorContainer container = await GetDependenciesContainerAsync(workspaceArtifactId).ConfigureAwait(false))
				{
					IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
					IList<Data.IntegrationPoint> integrationPoints = integrationPointService.GetAllIntegrationPoints();

					IList<IntegrationPointModel> result = new List<IntegrationPointModel>(integrationPoints.Count);
					foreach (var integrationPoint in integrationPoints)
					{
						IntegrationPointModel model = integrationPoint.ToIntegrationPointModel();
						result.Add(model);
					}

					return result;
				}
			}).ConfigureAwait(false);
		}

		private async Task<IWindsorContainer> GetDependenciesContainerAsync(int workspaceArtifactId)
		{
			return await Task.Run(() =>
			{
				IWindsorContainer container = new WindsorContainer();
#pragma warning disable 618
				ServiceInstaller installer = new ServiceInstaller(workspaceArtifactId);
#pragma warning restore 618
				installer.Install(container, new DefaultConfigurationStore());
				return container;
			}).ConfigureAwait(false);
		}

		public void Dispose()
		{
		}

		public async Task<int> GetSourceProviderArtifactIdAsync(int workspaceArtifactId, string sourceProviderGuidIdentifier)
		{
			using (IWindsorContainer container = await GetDependenciesContainerAsync(workspaceArtifactId).ConfigureAwait(false))
			{
				IRepositoryFactory repositoryFactory = container.Resolve<IRepositoryFactory>();
				ISourceProviderRepository sourceProviderRepository = repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
				int artifactId = sourceProviderRepository.GetArtifactIdFromSourceProviderTypeGuidIdentifier(sourceProviderGuidIdentifier);

				return artifactId;
			}
		}

		public async Task<int> GetIntegrationPointArtifactTypeIdAsync(int workspaceArtifactId)
		{
			using (IWindsorContainer container = await GetDependenciesContainerAsync(workspaceArtifactId).ConfigureAwait(false))
			{
				IObjectTypeRepository objectTypeRepository = container.Resolve<IObjectTypeRepository>();
				int? artifactTypeId = objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));
				if (artifactTypeId == null)
				{
					throw new Exception($"Unable to find the artifact type id of the integration point on workspace ${workspaceArtifactId}.");
				}
				return artifactTypeId.Value;
			}
		}
	}
}