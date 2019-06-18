using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public sealed class ValidationExecutorSystemTests : SystemTest
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
			_sourceWorkspace = sourceWorkspaceCreationTask.Result;
			_destinationWorkspace = destinationWorkspaceCreationTask.Result;
		}

		[IdentifiedTest("96d83692-044d-40f0-b335-84bc5a413478")]
		public async Task ItShouldSuccessfullyValidateJob()
		{
			int expectedSourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID;
			int expectedJobHistoryArtifactId = await Rdos.CreateJobHistoryInstance(ServiceFactory, expectedSourceWorkspaceArtifactId, _JOB_HISTORY_NAME).ConfigureAwait(false);
			int savedSearchArtifactId = await Rdos.GetSavedSearchInstance(ServiceFactory, expectedSourceWorkspaceArtifactId).ConfigureAwait(false);
			int destinationFolderArtifactId = await Rdos.GetRootFolderInstance(ServiceFactory, _destinationWorkspace.ArtifactID).ConfigureAwait(false);
			string folderPathSourceFieldName = await Rdos.GetFolderPathSourceFieldName(ServiceFactory, expectedSourceWorkspaceArtifactId).ConfigureAwait(false);

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
				JobName = _JOB_HISTORY_NAME,
				NotificationEmails = string.Empty,
				SavedSearchArtifactId = savedSearchArtifactId,
				DestinationFolderArtifactId = destinationFolderArtifactId,
				FieldMappings = _serializer.Deserialize<List<FieldMap>>(fieldsMap),
				FolderPathSourceFieldName = folderPathSourceFieldName,
				ImportOverwriteMode = ImportOverwriteMode.AppendOverlay,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.ReadFromField,
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings
			};

			// act
			ISyncJob syncJob = SyncJobHelper.CreateWithMockedProgressAndContainerExceptProvidedType<IValidationConfiguration>(configuration);

			// assert
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
		}
	}
}