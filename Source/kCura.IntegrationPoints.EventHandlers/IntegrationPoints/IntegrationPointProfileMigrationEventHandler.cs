using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("This is an event handler to update the Integration Point Profiles after workspace creation.")]
	[Guid("DC9F2F04-5095-4FAC-96A5-7D8A213A1463")]
	public class IntegrationPointProfileMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
	{
		private readonly Lazy<IRelativityObjectManagerFactory> _relativityObjectManagerFactory;
		private readonly IIntegrationPointProfilesQuery _integrationPointProfilesQuery;

		protected override string SuccessMessage => "Integration Point Profiles migrated successfully.";
		protected override string GetFailureMessage(Exception ex) => "Failed to migrate the Integration Point Profiles.";

		public IntegrationPointProfileMigrationEventHandler()
		{
			_relativityObjectManagerFactory = new Lazy<IRelativityObjectManagerFactory>(() => new RelativityObjectManagerFactory(Helper));
			Func<int, IRelativityObjectManager> createRelativityObjectManager = CreateRelativityObjectManager;
			var objectArtifactIdsByStringFieldValueQuery = new ObjectArtifactIdsByStringFieldValueQuery(createRelativityObjectManager);
			_integrationPointProfilesQuery = new IntegrationPointProfilesQuery(createRelativityObjectManager, objectArtifactIdsByStringFieldValueQuery);
		}

		internal IntegrationPointProfileMigrationEventHandler(IErrorService errorService,
			Func<IRelativityObjectManagerFactory> relativityObjectManagerFactoryProvider,
			IIntegrationPointProfilesQuery integrationPointProfilesQuery) : base(errorService)
		{
			_relativityObjectManagerFactory = new Lazy<IRelativityObjectManagerFactory>(relativityObjectManagerFactoryProvider);
			_integrationPointProfilesQuery = integrationPointProfilesQuery;
		}

		protected override void Run()
		{
			MigrateProfilesAsync().GetAwaiter().GetResult();
		}

		private async Task MigrateProfilesAsync()
		{
			int sourceProviderArtifactId = await _integrationPointProfilesQuery.GetSyncSourceProviderArtifactIdAsync(TemplateWorkspaceID).ConfigureAwait(false);
			int destinationProviderArtifactId = await _integrationPointProfilesQuery.GetSyncDestinationProviderArtifactIdAsync(TemplateWorkspaceID).ConfigureAwait(false);
			List<IntegrationPointProfile> allProfiles = (await _integrationPointProfilesQuery.GetAllProfilesAsync(TemplateWorkspaceID).ConfigureAwait(false)).ToList();
			List<int> syncProfilesArtifactIds = (await _integrationPointProfilesQuery
				.GetSyncProfilesAsync(allProfiles, sourceProviderArtifactId, destinationProviderArtifactId).ConfigureAwait(false)).ToList();
			List<int> nonSyncProfilesArtifactIds = (await _integrationPointProfilesQuery
				.GetNonSyncProfilesAsync(allProfiles, sourceProviderArtifactId, destinationProviderArtifactId).ConfigureAwait(false)).ToList();

			await Task.WhenAll(
					DeleteNonSyncProfilesIfAnyInCreatedWorkspaceAsync(nonSyncProfilesArtifactIds),
					ModifyExistingSyncProfilesIfAnyInCreatedWorkspaceAsync(syncProfilesArtifactIds)
				).ConfigureAwait(false);
		}

		private async Task DeleteNonSyncProfilesIfAnyInCreatedWorkspaceAsync(IReadOnlyCollection<int> nonSyncProfilesArtifactIds)
		{
			if (nonSyncProfilesArtifactIds.IsNullOrEmpty())
			{
				return;
			}

			bool success = await CreateRelativityObjectManager(WorkspaceId)
				.MassDeleteAsync(nonSyncProfilesArtifactIds)
				.ConfigureAwait(false);
			if (!success)
			{
				throw new IntegrationPointsException("Deleting non Sync Integration Point profiles failed");
			}
		}

		private Task ModifyExistingSyncProfilesIfAnyInCreatedWorkspaceAsync(IEnumerable<int> syncProfilesArtifactIds)
		{
			if (syncProfilesArtifactIds.IsNullOrEmpty())
			{
				return Task.CompletedTask;
			}

			return Task.CompletedTask; // NOTE: To be implemented in REL-351468
		}

		private IRelativityObjectManager CreateRelativityObjectManager(int workspaceId) =>
			_relativityObjectManagerFactory.Value.CreateRelativityObjectManager(workspaceId);

		private int WorkspaceId => Helper.GetActiveCaseID();
	}
}
