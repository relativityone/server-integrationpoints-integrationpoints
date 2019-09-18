using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("This is an event handler to update the Integration Point Profiles after workspace creation.")]
	[Guid("DC9F2F04-5095-4FAC-96A5-7D8A213A1463")]
	public class IntegrationPointProfileMigrationEventHandler : IntegrationPointMigrationEventHandlerBase
	{
		private readonly Lazy<IRelativityObjectManagerFactory> _relativityObjectManagerFactory;

		protected override string SuccessMessage => "Integration Point Profiles migrated successfully.";
		protected override string GetFailureMessage(Exception ex) => $"Failed to migrate the Integration Point Profiles, because: {ex.Message}";

		public IntegrationPointProfileMigrationEventHandler()
		{
			_relativityObjectManagerFactory = new Lazy<IRelativityObjectManagerFactory>(() => new RelativityObjectManagerFactory(Helper));
		}

		internal IntegrationPointProfileMigrationEventHandler(IErrorService errorService, Func<IRelativityObjectManagerFactory> relativityObjectManagerFactoryProvider)
			: base(errorService)
		{
			_relativityObjectManagerFactory = new Lazy<IRelativityObjectManagerFactory>(relativityObjectManagerFactoryProvider);
		}

		protected override void Run()
		{
			MigrateProfilesAsync().GetAwaiter().GetResult();
		}

		private async Task MigrateProfilesAsync()
		{
			var (nonSyncProfilesArtifactIds, syncProfilesArtifactIds) = await GetSyncAndNonSyncProfilesArtifactIdsAsync()
				.ConfigureAwait(false);

			await Task.WhenAll(
					DeleteNonSyncProfilesIfAnyInCreatedWorkspaceAsync(nonSyncProfilesArtifactIds),
					ModifyExistingSyncProfilesIfAnyInCreatedWorkspaceAsync(syncProfilesArtifactIds)
				).ConfigureAwait(false);
		}

		private async Task<(List<int> nonSyncProfilesArtifactIds, List<int> syncProfilesArtifactIds)> GetSyncAndNonSyncProfilesArtifactIdsAsync()
		{
			Task<int> syncDestinationProviderArtifactIdTask = GetSyncDestinationProviderArtifactIdAsync(TemplateWorkspaceID);
			Task<int> syncSourceProviderArtifactIdTask = GetSyncSourceProviderArtifactIdAsync(TemplateWorkspaceID);
			Task<List<IntegrationPointProfile>> getProfilesWithProvidersFromTemplateWorkspaceTask = GetProfilesWithProvidersFromTemplateWorkspaceAsync();
			await Task.WhenAll(
					syncSourceProviderArtifactIdTask,
					syncDestinationProviderArtifactIdTask,
					getProfilesWithProvidersFromTemplateWorkspaceTask)
				.ConfigureAwait(false);

			bool IsSyncProfile(IntegrationPointProfile integrationPointProfile) =>
				integrationPointProfile.DestinationProvider == syncDestinationProviderArtifactIdTask.Result &&
				integrationPointProfile.SourceProvider == syncSourceProviderArtifactIdTask.Result;

			List<IntegrationPointProfile> allProfiles = getProfilesWithProvidersFromTemplateWorkspaceTask.Result;

			List<int> nonSyncProfilesArtifactIds = allProfiles
				.Where(p => !IsSyncProfile(p))
				.Select(p => p.ArtifactId)
				.ToList();
			List<int> syncProfilesArtifactIds = allProfiles
				.Where(p => IsSyncProfile(p))
				.Select(p => p.ArtifactId)
				.ToList();
			return (nonSyncProfilesArtifactIds, syncProfilesArtifactIds);
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

		private async Task ModifyExistingSyncProfilesIfAnyInCreatedWorkspaceAsync(IEnumerable<int> syncProfilesArtifactIds)
		{
			if (syncProfilesArtifactIds.IsNullOrEmpty())
			{
				return;
			}

			await Task.Yield(); // NOTE: To be implemented in REL-351468
		}

		private Task<int> GetSyncDestinationProviderArtifactIdAsync(int workspaceId) =>
			GetSingleObjectArtifactIdByStringFieldValueAsync<DestinationProvider>(workspaceId,
				destinationProvider => destinationProvider.Identifier,
				Constants.IntegrationPoints.DestinationProviders.RELATIVITY);

		private Task<int> GetSyncSourceProviderArtifactIdAsync(int workspaceId) =>
			GetSingleObjectArtifactIdByStringFieldValueAsync<SourceProvider>(workspaceId,
				sourceProvider => sourceProvider.Identifier,
				Constants.IntegrationPoints.SourceProviders.RELATIVITY);

		private async Task<int> GetSingleObjectArtifactIdByStringFieldValueAsync<TSource>(int workspaceId,
			Expression<Func<TSource, string>> propertySelector, string fieldValue) where TSource : BaseRdo, new()
		{
			List<int> objectsArtifactIds = await CreateRelativityObjectManager(workspaceId)
				.GetObjectArtifactIdsByStringFieldValueAsync(propertySelector, fieldValue)
				.ConfigureAwait(false);

			int artifactId = objectsArtifactIds.Single();
			return artifactId;
		}

		private async Task<List<IntegrationPointProfile>> GetProfilesWithProvidersFromTemplateWorkspaceAsync()
		{
			var queryRequest = new QueryRequest
			{
				Fields = new[]
				{
					new FieldRef
					{
						Guid = IntegrationPointProfileFieldGuids.DestinationProviderGuid
					},
					new FieldRef
					{
						Guid = IntegrationPointProfileFieldGuids.SourceProviderGuid
					}
				}
			};
			List<IntegrationPointProfile> integrationPointProfiles = await CreateRelativityObjectManager(TemplateWorkspaceID)
				.QueryAsync<IntegrationPointProfile>(queryRequest)
				.ConfigureAwait(false);

			return integrationPointProfiles;
		}

		private IRelativityObjectManager CreateRelativityObjectManager(int workspaceId) =>
			_relativityObjectManagerFactory.Value.CreateRelativityObjectManager(workspaceId);

		private int WorkspaceId => Helper.GetActiveCaseID();
	}
}
