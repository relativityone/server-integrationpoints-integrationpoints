using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Installers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Helpers;
using kCura.IntegrationPoints.Services.Repositories;
using Relativity.Logging;

namespace kCura.IntegrationPoints.Services
{
	public class ProviderManager : KeplerServiceBase, IProviderManager
	{
		private Installer _installer;

		/// <summary>
		///     For testing purposes only
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="permissionRepositoryFactory"></param>
		/// <param name="container"></param>
		internal ProviderManager(ILog logger, IPermissionRepositoryFactory permissionRepositoryFactory, IWindsorContainer container)
			: base(logger, permissionRepositoryFactory, container)
		{
		}

		public ProviderManager(ILog logger) : base(logger)
		{
		}

		protected override Installer Installer => _installer ?? (_installer = new ProviderManagerInstaller());

		public void Dispose()
		{
		}

		public async Task<int> GetSourceProviderArtifactIdAsync(int workspaceArtifactId, string sourceProviderGuidIdentifier)
		{
			CheckSourceProviderPermissions(workspaceArtifactId, nameof(GetSourceProviderArtifactIdAsync));
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var providerRepository = container.Resolve<IProviderRepository>();
					return await Task.Run(() => providerRepository.GetSourceProviderArtifactId(workspaceArtifactId, sourceProviderGuidIdentifier)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetSourceProviderArtifactIdAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<int> GetDestinationProviderArtifactIdAsync(int workspaceArtifactId, string destinationProviderGuidIdentifier)
		{
			CheckDestinationProviderPermissions(workspaceArtifactId, nameof(GetDestinationProviderArtifactIdAsync));
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var providerRepository = container.Resolve<IProviderRepository>();
					return await Task.Run(() => providerRepository.GetDestinationProviderArtifactId(workspaceArtifactId, destinationProviderGuidIdentifier)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetDestinationProviderArtifactIdAsync), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IList<ProviderModel>> GetSourceProviders(int workspaceArtifactId)
		{
			CheckSourceProviderPermissions(workspaceArtifactId, nameof(GetSourceProviders));
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var providerRepository = container.Resolve<IProviderRepository>();
					return await Task.Run(() => providerRepository.GetSourceProviders(workspaceArtifactId)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetSourceProviders), e);
				throw CreateInternalServerErrorException();
			}
		}

		public async Task<IList<ProviderModel>> GetDestinationProviders(int workspaceArtifactId)
		{
			CheckDestinationProviderPermissions(workspaceArtifactId, nameof(GetDestinationProviders));
			try
			{
				using (var container = GetDependenciesContainer(workspaceArtifactId))
				{
					var providerRepository = container.Resolve<IProviderRepository>();
					return await Task.Run(() => providerRepository.GetDesinationProviders(workspaceArtifactId)).ConfigureAwait(false);
				}
			}
			catch (Exception e)
			{
				LogException(nameof(GetDestinationProviders), e);
				throw CreateInternalServerErrorException();
			}
		}

		private void CheckSourceProviderPermissions(int workspaceId, string endpointName)
		{
			SafePermissionCheck(() =>
			{
				var permissionRepository = GetPermissionRepository(workspaceId);
				bool hasWorkspaceAccess = permissionRepository.UserHasPermissionToAccessWorkspace();
				bool hasSourceProviderViewAccess = permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View);
				if (hasWorkspaceAccess && hasSourceProviderViewAccess)
				{
					return;
				}
				IList<string> missingPermissions = new List<string>();
				if (!hasWorkspaceAccess)
				{
					missingPermissions.Add("Workspace");
				}
				if (!hasSourceProviderViewAccess)
				{
					missingPermissions.Add($"{ObjectTypes.SourceProvider} - View");
				}
				LogAndThrowInsufficientPermissionException(endpointName, missingPermissions);
			});
		}

		private void CheckDestinationProviderPermissions(int workspaceId, string endpointName)
		{
			SafePermissionCheck(() =>
			{
				var permissionRepository = GetPermissionRepository(workspaceId);
				bool hasWorkspaceAccess = permissionRepository.UserHasPermissionToAccessWorkspace();
				bool hasDestinationProviderViewAccess = permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View);
				if (hasWorkspaceAccess && hasDestinationProviderViewAccess)
				{
					return;
				}
				IList<string> missingPermissions = new List<string>();
				if (!hasWorkspaceAccess)
				{
					missingPermissions.Add("Workspace");
				}
				if (!hasDestinationProviderViewAccess)
				{
					missingPermissions.Add($"{ObjectTypes.DestinationProvider} - View");
				}
				LogAndThrowInsufficientPermissionException(endpointName, missingPermissions);
			});
		}
	}
}