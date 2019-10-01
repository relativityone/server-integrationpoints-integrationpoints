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
using Newtonsoft.Json.Linq;
using Relativity.Services.Objects.DataContracts;

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
			int sourceProviderArtifactID = await _integrationPointProfilesQuery.GetSyncSourceProviderArtifactIdAsync(TemplateWorkspaceID).ConfigureAwait(false);
			int destinationProviderArtifactID = await _integrationPointProfilesQuery.GetSyncDestinationProviderArtifactIdAsync(TemplateWorkspaceID).ConfigureAwait(false);
			List<IntegrationPointProfile> allProfiles = (await _integrationPointProfilesQuery.GetAllProfilesAsync(TemplateWorkspaceID).ConfigureAwait(false)).ToList();
			List<IntegrationPointProfile> syncProfiles = (await _integrationPointProfilesQuery
				.GetSyncProfilesAsync(allProfiles, sourceProviderArtifactID, destinationProviderArtifactID).ConfigureAwait(false)).ToList();
			List<IntegrationPointProfile> nonSyncProfiles = (await _integrationPointProfilesQuery
				.GetNonSyncProfilesAsync(allProfiles, sourceProviderArtifactID, destinationProviderArtifactID).ConfigureAwait(false)).ToList();

			await Task.WhenAll(
					DeleteNonSyncProfilesIfAnyInCreatedWorkspaceAsync(nonSyncProfiles),
					ModifyExistingSyncProfilesIfAnyInCreatedWorkspaceAsync(syncProfiles)
				).ConfigureAwait(false);
		}

		private async Task DeleteNonSyncProfilesIfAnyInCreatedWorkspaceAsync(IReadOnlyCollection<IntegrationPointProfile> nonSyncProfiles)
		{
			if (nonSyncProfiles.IsNullOrEmpty())
			{
				return;
			}

			bool success = await CreateRelativityObjectManager(WorkspaceID)
				.MassDeleteAsync(nonSyncProfiles.Select(x => x.ArtifactId))
				.ConfigureAwait(false);
			if (!success)
			{
				throw new IntegrationPointsException("Deleting non Sync Integration Point profiles failed");
			}
		}

		private async Task ModifyExistingSyncProfilesIfAnyInCreatedWorkspaceAsync(IReadOnlyCollection<IntegrationPointProfile> syncProfiles)
		{
			if (syncProfiles.IsNullOrEmpty())
			{
				return;
			}

			IRelativityObjectManager objectManager = CreateRelativityObjectManager(WorkspaceID);

			int sourceProviderArtifactId = await _integrationPointProfilesQuery.GetSyncSourceProviderArtifactIdAsync(WorkspaceID).ConfigureAwait(false);
			int destinationProviderArtifactId = await _integrationPointProfilesQuery.GetSyncDestinationProviderArtifactIdAsync(WorkspaceID).ConfigureAwait(false);
			int integrationPointTypeArtifactId = await _integrationPointProfilesQuery.GetIntegrationPointExportTypeArtifactIdAsync(WorkspaceID).ConfigureAwait(false);

			foreach (IntegrationPointProfile profile in syncProfiles)
			{
				bool success = await objectManager.MassUpdateAsync(new[] {profile.ArtifactId}, new[]
				{
					new FieldRefValuePair()
					{
						Field = new FieldRef()
						{
							Guid = IntegrationPointProfileFieldGuids.SourceConfigurationGuid
						},
						Value = UpdateSourceConfiguration(profile.SourceConfiguration)
					},
					new FieldRefValuePair()
					{
						Field = new FieldRef
						{
							Guid = IntegrationPointProfileFieldGuids.SourceProviderGuid
						},
						Value = new RelativityObjectRef()
						{
							ArtifactID = sourceProviderArtifactId
						}
					},
					new FieldRefValuePair()
					{
						Field = new FieldRef
						{
							Guid = IntegrationPointProfileFieldGuids.DestinationProviderGuid
						},
						Value = new RelativityObjectRef()
						{
							ArtifactID = destinationProviderArtifactId
						}
					}, 
					new FieldRefValuePair()
					{
						Field = new FieldRef()
						{
							Guid = IntegrationPointProfileFieldGuids.TypeGuid
						},
						Value = new RelativityObjectRef()
						{
							ArtifactID = integrationPointTypeArtifactId
						}
					}
				}, FieldUpdateBehavior.Replace).ConfigureAwait(false);

				if (!success)
				{
					throw new IntegrationPointsException("Updating Integration Point Profile with ObjectManager failed.");
				}
			}
		}

		private string UpdateSourceConfiguration(string sourceConfiguration)
		{
			JObject configJson = JObject.Parse(sourceConfiguration);
			configJson.Property(nameof(kCura.IntegrationPoints.Core.Contracts.Configuration.SourceConfiguration.SavedSearchArtifactId)).Value = null;
			configJson.Property(nameof(kCura.IntegrationPoints.Core.Contracts.Configuration.SourceConfiguration.SourceWorkspaceArtifactId)).Value = WorkspaceID;
			return configJson.ToString();
		}
		
		private IRelativityObjectManager CreateRelativityObjectManager(int workspaceID) =>
			_relativityObjectManagerFactory.Value.CreateRelativityObjectManager(workspaceID);

		private int WorkspaceID => Helper.GetActiveCaseID();
	}
}
