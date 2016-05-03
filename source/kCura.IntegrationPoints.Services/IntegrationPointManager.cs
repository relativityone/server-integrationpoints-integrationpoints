using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;

namespace kCura.IntegrationPoints.Services
{
	public class IntegrationPointManager : KelperServiceBase, IIntegrationPointManager
	{
		public async Task<IntegrationPointModel> CreateIntegrationPointAsync(CreateIntegrationPointRequest request)
		{
			return await Task.Run(async () =>
			{
				IWindsorContainer container = await GetDependenciesContainerAsync(request.WorkspaceArtifactId).ConfigureAwait(false);
				IntegrationPointModel returnVal = new IntegrationPointModel();

				request.ValidateRequest();
				request.ValidatePermission(container);

				IntegrationModel ip = request.CreateIntegrationPointModel();
				IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();

				returnVal.ArtifactId = integrationPointService.SaveIntegration(ip);
				returnVal.Name = request.Name;

				return returnVal;
			}).ConfigureAwait(false);
		}

		public async Task<IntegrationPointModel> UpdateIntegrationPointAsync(UpdateIntegrationPointRequest request)
		{
			return await Task.Run(async () =>
			{
				IWindsorContainer container = await GetDependenciesContainerAsync(request.WorkspaceArtifactId).ConfigureAwait(false);
				IntegrationPointModel returnVal = new IntegrationPointModel();

				request.ValidateRequest();
				request.ValidatePermission(container);

				IntegrationModel ip = request.CreateIntegrationPointModel();
				IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
				if (integrationPointService.GetRdo(request.IntegrationPointArtifactId) == null)
				{
					throw new Exception("requested object does not exist.");
				}
				returnVal.ArtifactId = integrationPointService.SaveIntegration(ip);
				returnVal.Name = request.Name;

				return returnVal;
			}).ConfigureAwait(false);
		}

		public async Task<IntegrationPointModel> GetIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId)
		{
			return await Task.Run(async () =>
			{
				IWindsorContainer container = await GetDependenciesContainerAsync(workspaceArtifactId).ConfigureAwait(false);

				IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
				Data.IntegrationPoint integrationPoint = integrationPointService.GetRdo(integrationPointArtifactId);

				var result = new IntegrationPointModel()
				{
					ArtifactId = integrationPoint.ArtifactId,
					Name = integrationPoint.Name
				};
				return result;
			}).ConfigureAwait(false);
		}

		public async Task RunIntegrationPointAsync(int workspaceArtifactId, int integrationPointArtifactId)
		{
			IWindsorContainer container = await GetDependenciesContainerAsync(workspaceArtifactId).ConfigureAwait(false);
			IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
			int userId = global::Relativity.API.Services.Helper.GetAuthenticationManager().UserInfo.ArtifactID;
			integrationPointService.RunIntegrationPoint(workspaceArtifactId, integrationPointArtifactId, userId);
		}

		public async Task<IList<IntegrationPointModel>> GetAllIntegrationPointsAsync(int workspaceArtifactId)
		{
			return await Task.Run(async () =>
			{
				IList<IntegrationPointModel> result = new List<IntegrationPointModel>();

				IWindsorContainer container = await GetDependenciesContainerAsync(workspaceArtifactId).ConfigureAwait(false);
				IIntegrationPointService integrationPointService = container.Resolve<IIntegrationPointService>();
				IList<Data.IntegrationPoint> integrationPoints = integrationPointService.GetAllIntegrationPoints();

				foreach (var integrationPoint in integrationPoints)
				{
					result.Add(
					new IntegrationPointModel()
					{
						Name = integrationPoint.Name,
						ArtifactId = integrationPoint.ArtifactId
					});
				}

				return result;
			}).ConfigureAwait(false);
		}

		private async Task<IWindsorContainer> GetDependenciesContainerAsync(int workspace)
		{
			return await Task.Run(() =>
			{
				IWindsorContainer container = new WindsorContainer();
				ServiceInstaller installer = new ServiceInstaller(workspace);
				installer.Install(container, new DefaultConfigurationStore());
				return container;
			}).ConfigureAwait(false);
		}

		public void Dispose()
		{
		}
	}
}