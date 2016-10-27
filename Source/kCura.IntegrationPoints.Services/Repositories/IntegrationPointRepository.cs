using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Interfaces.Private.Extensions;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public class IntegrationPointRepository : IIntegrationPointRepository
	{
		private readonly ILog _logger;

		public IntegrationPointRepository(ILog logger)
		{
			_logger = logger;
		}

		public async Task<IntegrationPointModel> CreateIntegrationPointAsync(CreateIntegrationPointRequest request)
		{
			try
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
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{}.{}", nameof(IntegrationPointManager), nameof(CreateIntegrationPointAsync));
				throw;
			}
		}

		public async Task<IntegrationPointModel> UpdateIntegrationPointAsync(UpdateIntegrationPointRequest request)
		{
			try
			{
				using (IWindsorContainer container = await GetDependenciesContainerAsync(request.WorkspaceArtifactId).ConfigureAwait(false))
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
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{}.{}", nameof(IntegrationPointManager), nameof(UpdateIntegrationPointAsync));
				throw;
			}
		}

		public async Task<IntegrationPointModel> GetIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId)
		{
			try
			{
				using (IWindsorContainer container = await GetDependenciesContainerAsync(workspaceArtifactId).ConfigureAwait(false))
				{
					IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
					IntegrationPoint integrationPoint = integrationPointService.GetRdo(integrationPointArtifactId);
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
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{}.{}", nameof(IntegrationPointManager), nameof(GetIntegrationPointAsync));
				throw;
			}
		}

		public async Task RunIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId)
		{
			try
			{
				using (IWindsorContainer container = await GetDependenciesContainerAsync(workspaceArtifactId).ConfigureAwait(false))
				{
					IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
					int userId = global::Relativity.API.Services.Helper.GetAuthenticationManager().UserInfo.ArtifactID;
					integrationPointService.RunIntegrationPoint(workspaceArtifactId, integrationPointArtifactId, userId);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{}.{}", nameof(IntegrationPointManager), nameof(RunIntegrationPointAsync));
				throw;
			}
		}

		public async Task<IList<IntegrationPointModel>> GetAllIntegrationPointsAsync(int workspaceArtifactId)
		{
			try
			{
				using (IWindsorContainer container = await GetDependenciesContainerAsync(workspaceArtifactId).ConfigureAwait(false))
				{
					IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
					IList<IntegrationPoint> integrationPoints = integrationPointService.GetAllIntegrationPoints();

					IList<IntegrationPointModel> result = new List<IntegrationPointModel>(integrationPoints.Count);
					foreach (var integrationPoint in integrationPoints)
					{
						IntegrationPointModel model = integrationPoint.ToIntegrationPointModel();
						result.Add(model);
					}

					return result;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{}.{}", nameof(IntegrationPointManager), nameof(GetAllIntegrationPointsAsync));
				throw;
			}
		}

		public async Task<int> GetSourceProviderArtifactIdAsync(int workspaceArtifactId, string sourceProviderGuidIdentifier)
		{
			try
			{
				using (IWindsorContainer container = await GetDependenciesContainerAsync(workspaceArtifactId).ConfigureAwait(false))
				{
					IRepositoryFactory repositoryFactory = container.Resolve<IRepositoryFactory>();
					ISourceProviderRepository sourceProviderRepository = repositoryFactory.GetSourceProviderRepository(workspaceArtifactId);
					int artifactId = sourceProviderRepository.GetArtifactIdFromSourceProviderTypeGuidIdentifier(sourceProviderGuidIdentifier);

					return artifactId;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "{}.{}", nameof(IntegrationPointManager), nameof(GetSourceProviderArtifactIdAsync));
				throw;
			}
		}

		public async Task<int> GetIntegrationPointArtifactTypeIdAsync(int workspaceArtifactId)
		{
			try
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
			catch (Exception ex)
			{
				_logger.LogError(ex, "{}.{}", nameof(IntegrationPointManager), nameof(GetIntegrationPointArtifactTypeIdAsync));
				throw;
			}
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
	}
}