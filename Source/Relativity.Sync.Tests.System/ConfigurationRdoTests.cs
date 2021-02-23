using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class ConfigurationRdoTests : SystemTest
	{
		private ISourceServiceFactoryForAdmin _serviceFactory;

		private int _workspaceId;
		private int _jobHistoryArtifactId;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			_serviceFactory = new ServiceFactoryStub(ServiceFactory);

			WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			_workspaceId = workspace.ArtifactID;

			_jobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _workspaceId).ConfigureAwait(false);
		}

		[IdentifiedTest("e5a86b87-b1e8-4de3-b4fa-5f3e3878be49")]
		public async Task ItShouldContainAllFields()
		{
			CreateRequest request = new CreateRequest
			{
				ParentObject = new RelativityObjectRef
				{
					ArtifactID = _jobHistoryArtifactId
				},
				ObjectType = new ObjectTypeRef
				{
					Guid = SyncRdoGuids.SyncConfigurationGuid
				},
				FieldValues = PrepareFields().ToList()
			};

			int syncConfigurationArtifactId;
			using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				CreateResult result = await objectManager.CreateAsync(_workspaceId, request).ConfigureAwait(false);
				syncConfigurationArtifactId = result.Object.ArtifactID;
			}

			SyncJobParameters jobParameters = new SyncJobParameters(syncConfigurationArtifactId, _workspaceId,_jobHistoryArtifactId);
			Storage.IConfiguration configuration = await Storage.Configuration
				.GetAsync(_serviceFactory, jobParameters, new EmptyLogger(), new SemaphoreSlimWrapper(new SemaphoreSlim(1))).ConfigureAwait(false);

			// ASSERT
			configuration.GetFieldValue<bool>(SyncRdoGuids.CreateSavedSearchInDestinationGuid);
			configuration.GetFieldValue<int>(SyncRdoGuids.DataDestinationArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.DataDestinationTypeGuid);
			configuration.GetFieldValue<int>(SyncRdoGuids.DataSourceArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.DataSourceTypeGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.DestinationFolderStructureBehaviorGuid);
			configuration.GetFieldValue<int>(SyncRdoGuids.DestinationWorkspaceArtifactIdGuid);
			configuration.GetFieldValue<int>(SyncRdoGuids.DestinationWorkspaceTagArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.EmailNotificationRecipientsGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.FieldMappingsGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.FieldOverlayBehaviorGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.FolderPathSourceFieldNameGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.ImportOverwriteModeGuid);
			configuration.GetFieldValue<RelativityObjectValue>(JobHistoryGuid);
			configuration.GetFieldValue<RelativityObjectValue>(SyncRdoGuids.JobHistoryToRetryIdGuid);
			configuration.GetFieldValue<bool>(SyncRdoGuids.MoveExistingDocumentsGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.NativesBehaviorGuid);
			configuration.GetFieldValue<int>(SyncRdoGuids.RdoArtifactTypeIdGuid);
			configuration.GetFieldValue<int>(SyncRdoGuids.SavedSearchInDestinationArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.SnapshotIdGuid);
			configuration.GetFieldValue<int>(SyncRdoGuids.SnapshotRecordsCountGuid);
			configuration.GetFieldValue<int>(SyncRdoGuids.SourceJobTagArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.SourceJobTagNameGuid);
			configuration.GetFieldValue<int>(SyncRdoGuids.SourceWorkspaceTagArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.SourceWorkspaceTagNameGuid);

			configuration.GetFieldValue<bool>(SyncRdoGuids.ImageImportGuid);
			configuration.GetFieldValue<bool>(SyncRdoGuids.IncludeOriginalImagesGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.ImageFileCopyModeGuid);
			configuration.GetFieldValue<string>(SyncRdoGuids.ProductionImagePrecedenceGuid);
		}

		[IdentifiedTest("3a5a8647-86e5-4a01-9cac-90bdd84e7a62")]
		public async Task ItShouldHandleLongTextFromStream()
		{
			const string fieldMapping = "in theory, very long text, but we check for ellipsis really...";
			CreateRequest request = new CreateRequest
			{
				ParentObject = new RelativityObjectRef
				{
					ArtifactID = _jobHistoryArtifactId
				},
				ObjectType = new ObjectTypeRef
				{
					Guid = SyncRdoGuids.SyncConfigurationGuid
				},
				FieldValues = PrepareFields().ToList()
			};

			request.FieldValues.First(x => x.Field.Guid == SyncRdoGuids.FieldMappingsGuid).Value = fieldMapping;

			int syncConfigurationArtifactId;
			using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				CreateResult result = await objectManager.CreateAsync(_workspaceId, request).ConfigureAwait(false);
				syncConfigurationArtifactId = result.Object.ArtifactID;
			}

			SyncJobParameters jobParameters = new SyncJobParameters(syncConfigurationArtifactId, _workspaceId, _jobHistoryArtifactId);
			Storage.IConfiguration configuration = await Storage.Configuration
				.GetAsync(_serviceFactory, jobParameters, new EmptyLogger(), new SemaphoreSlimWrapper(new SemaphoreSlim(1))).ConfigureAwait(false);

			// ASSERT
			configuration.GetFieldValue<string>(SyncRdoGuids.FieldMappingsGuid).Should().Be(fieldMapping);
		}

		[IdentifiedTest("108661af-82bf-41ca-8799-4c83cc0ef976")]
		public async Task FieldsShouldBeOfValidTypes()
		{
			CreateRequest request = new CreateRequest
			{
				ParentObject = new RelativityObjectRef
				{
					ArtifactID = _jobHistoryArtifactId
				},
				ObjectType = new ObjectTypeRef
				{
					Guid = SyncRdoGuids.SyncConfigurationGuid
				},
				FieldValues = PrepareFields().ToList()
			};

			using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				await objectManager.CreateAsync(_workspaceId, request).ConfigureAwait(false);
			}
		}

		private static IEnumerable<FieldRefValuePair> PrepareFields()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.CreateSavedSearchInDestinationGuid
				},
				Value = true
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.DataDestinationArtifactIdGuid
				},
				Value = 789456
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.DataDestinationTypeGuid
				},
				Value = "some type"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.DataSourceArtifactIdGuid
				},
				Value = 589632
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.DataSourceTypeGuid
				},
				Value = "source type"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.DestinationFolderStructureBehaviorGuid
				},
				Value = "behavior"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.DestinationWorkspaceArtifactIdGuid
				},
				Value = 784512
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.DestinationWorkspaceTagArtifactIdGuid
				},
				Value = 968574
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.EmailNotificationRecipientsGuid
				},
				Value = "emails"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.FieldMappingsGuid
				},
				Value = "fields"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.FieldOverlayBehaviorGuid
				},
				Value = "overlay behavior"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.FolderPathSourceFieldNameGuid
				},
				Value = "folder path"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.ImportOverwriteModeGuid
				},
				Value = "overwrite mode"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.MoveExistingDocumentsGuid
				},
				Value = true
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.NativesBehaviorGuid
				},
				Value = "natives behavior"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.RdoArtifactTypeIdGuid
				},
				Value = 784512
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.SavedSearchInDestinationArtifactIdGuid
				},
				Value = 789632
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.SnapshotIdGuid
				},
				Value = "snapshot id"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.SnapshotRecordsCountGuid
				},
				Value = 813794
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.SourceJobTagArtifactIdGuid
				},
				Value = 951357
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.SourceJobTagNameGuid
				},
				Value = "source tag name"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.SourceWorkspaceTagArtifactIdGuid
				},
				Value = 659823
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncRdoGuids.SourceWorkspaceTagNameGuid
				},
				Value = "workspace tag name"
			};
			yield return new FieldRefValuePair()
			{
				Field = new FieldRef()
				{
					Guid = SyncRdoGuids.ImageImportGuid
				},
				Value = true
			};
			yield return new FieldRefValuePair()
			{
				Field = new FieldRef()
				{
					Guid = SyncRdoGuids.IncludeOriginalImagesGuid
				},
				Value = true
			};
			yield return new FieldRefValuePair()
			{
				Field = new FieldRef()
				{
					Guid = SyncRdoGuids.ImageFileCopyModeGuid
				},
				Value = "Links"
			};
			yield return new FieldRefValuePair()
			{
				Field = new FieldRef()
				{
					Guid = SyncRdoGuids.ProductionImagePrecedenceGuid
				},
				Value = "[2564218,254634]"
			};


#pragma warning restore RG2009 // Hardcoded Numeric Value
		}
	}
}