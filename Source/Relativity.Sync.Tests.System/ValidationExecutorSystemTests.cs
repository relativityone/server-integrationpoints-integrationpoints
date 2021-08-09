using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Utils;
using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;
using FluentAssertions;
using System;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class ValidationExecutorSystemTests : SystemTest
	{
		private WorkspaceRef _destinationWorkspace;
		private WorkspaceRef _sourceWorkspace;

		private JSONSerializer _serializer;

		private const string _JOB_HISTORY_NAME = "Test Job Name";

		[SetUp]
		public async Task SetUp()
		{
			_serializer = new JSONSerializer();
			Task<WorkspaceRef> sourceWorkspaceCreationTask = Environment.CreateWorkspaceWithFieldsAsync();
			Task<WorkspaceRef> destinationWorkspaceCreationTask = Environment.CreateWorkspaceAsync();
			await Task.WhenAll(sourceWorkspaceCreationTask, destinationWorkspaceCreationTask).ConfigureAwait(false);
			_sourceWorkspace = sourceWorkspaceCreationTask.GetAwaiter().GetResult();
			_destinationWorkspace = destinationWorkspaceCreationTask.GetAwaiter().GetResult();
		}

		[IdentifiedTest("96d83692-044d-40f0-b335-84bc5a413478")]
		public async Task ItShouldSuccessfullyValidateJob()
		{
			int expectedSourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID;
			int expectedJobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, expectedSourceWorkspaceArtifactId, _JOB_HISTORY_NAME).ConfigureAwait(false);
			int savedSearchArtifactId = await Rdos.GetSavedSearchInstanceAsync(ServiceFactory, expectedSourceWorkspaceArtifactId).ConfigureAwait(false);
			int destinationFolderArtifactId = await Rdos.GetRootFolderInstanceAsync(ServiceFactory, _destinationWorkspace.ArtifactID).ConfigureAwait(false);
			string folderPathSourceFieldName = await Rdos.GetFolderPathSourceFieldNameAsync(ServiceFactory, expectedSourceWorkspaceArtifactId).ConfigureAwait(false);

			const string fieldsMap =
				"[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true," +
				"\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"destinationField\":" +
				"{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\"," +
				"\"isRequired\":true},\"fieldMapType\":\"Identifier\"}]";

			ConfigurationStub configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = expectedSourceWorkspaceArtifactId,
				JobHistoryArtifactId = expectedJobHistoryArtifactId,
				SavedSearchArtifactId = savedSearchArtifactId,
				DestinationFolderArtifactId = destinationFolderArtifactId,
				FolderPathSourceFieldName = folderPathSourceFieldName,
				ImportOverwriteMode = ImportOverwriteMode.AppendOverlay,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.ReadFromField,
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings
			};
			configuration.SetFieldMappings(_serializer.Deserialize<List<FieldMap>>(fieldsMap));
			configuration.SetJobName(_JOB_HISTORY_NAME);

			// act
			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IValidationConfiguration>(configuration);

			// assert
			await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);
		}

		[IdentifiedTest("E070BD2B-BEAB-4304-A175-434D8EBA7348")]
		public async Task ItShouldMarkJobAsInvalid_WhenFieldHasBeenRenamed()
		{
			int expectedSourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID;
			int expectedJobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, expectedSourceWorkspaceArtifactId, _JOB_HISTORY_NAME).ConfigureAwait(false);
			int savedSearchArtifactId = await Rdos.GetSavedSearchInstanceAsync(ServiceFactory, expectedSourceWorkspaceArtifactId).ConfigureAwait(false);
			int destinationFolderArtifactId = await Rdos.GetRootFolderInstanceAsync(ServiceFactory, _destinationWorkspace.ArtifactID).ConfigureAwait(false);
			string folderPathSourceFieldName = await Rdos.GetFolderPathSourceFieldNameAsync(ServiceFactory, expectedSourceWorkspaceArtifactId).ConfigureAwait(false);

			const string fieldsMap =
				@"[
					{
						""sourceField"": { 
							""displayName"":""Control Number"",
							""isIdentifier"":true,
							""fieldIdentifier"":""1003667"",
							""isRequired"":true,
						},
						""destinationField"": {
							""displayName"":""Control Number"",
							""isIdentifier"":true,
							""fieldIdentifier"":""1003667"",
							""isRequired"":true,
						},
						""fieldMapType"":""Identifier""
					},
					{
						""sourceField"": {
							""displayName"":""Extracted Text"",
							""isIdentifier"":false,
							""fieldIdentifier"":""1003668"",
							""isRequired"":false,
						},
						""destinationField"": {
							""displayName"":""Extracted Text - RENAMED"",
							""isIdentifier"":false,
							""fieldIdentifier"":""1003668"",
							""isRequired"":false,
						},
						""fieldMapType"":""None""
					}
				]";

			ConfigurationStub configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID,
				SourceWorkspaceArtifactId = expectedSourceWorkspaceArtifactId,
				JobHistoryArtifactId = expectedJobHistoryArtifactId,
				SavedSearchArtifactId = savedSearchArtifactId,
				DestinationFolderArtifactId = destinationFolderArtifactId,
				FolderPathSourceFieldName = folderPathSourceFieldName,
				ImportOverwriteMode = ImportOverwriteMode.AppendOverlay,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.ReadFromField,
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings
			};
			configuration.SetFieldMappings(_serializer.Deserialize<List<FieldMap>>(fieldsMap));
			configuration.SetJobName(_JOB_HISTORY_NAME);

			// act
			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IValidationConfiguration>(configuration);
			Func<Task> executeJob = () => syncJob.ExecuteAsync(CompositeCancellationToken.None);

			// assert
			executeJob.Should().Throw<ValidationException>();
		}
	}
}