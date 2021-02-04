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
					Guid = SyncConfigurationRdo.SyncConfigurationGuid
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
			configuration.GetFieldValue<bool>(SyncConfigurationRdo.CreateSavedSearchInDestinationGuid);
			configuration.GetFieldValue<int>(SyncConfigurationRdo.DataDestinationArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.DataDestinationTypeGuid);
			configuration.GetFieldValue<int>(SyncConfigurationRdo.DataSourceArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.DataSourceTypeGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.DestinationFolderStructureBehaviorGuid);
			configuration.GetFieldValue<int>(SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid);
			configuration.GetFieldValue<int>(SyncConfigurationRdo.DestinationWorkspaceTagArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.EmailNotificationRecipientsGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.FieldMappingsGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.FieldOverlayBehaviorGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.FolderPathSourceFieldNameGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.ImportOverwriteModeGuid);
			configuration.GetFieldValue<RelativityObjectValue>(JobHistoryGuid);
			configuration.GetFieldValue<RelativityObjectValue>(SyncConfigurationRdo.JobHistoryToRetryIdGuid);
			configuration.GetFieldValue<bool>(SyncConfigurationRdo.MoveExistingDocumentsGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.NativesBehaviorGuid);
			configuration.GetFieldValue<int>(SyncConfigurationRdo.RdoArtifactTypeIdGuid);
			configuration.GetFieldValue<int>(SyncConfigurationRdo.SavedSearchInDestinationArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.SnapshotIdGuid);
			configuration.GetFieldValue<int>(SyncConfigurationRdo.SnapshotRecordsCountGuid);
			configuration.GetFieldValue<int>(SyncConfigurationRdo.SourceJobTagArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.SourceJobTagNameGuid);
			configuration.GetFieldValue<int>(SyncConfigurationRdo.SourceWorkspaceTagArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.SourceWorkspaceTagNameGuid);

			configuration.GetFieldValue<bool>(SyncConfigurationRdo.ImageImportGuid);
			configuration.GetFieldValue<bool>(SyncConfigurationRdo.IncludeOriginalImagesGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.ImageFileCopyModeGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.ProductionImagePrecedenceGuid);
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
					Guid = SyncConfigurationRdo.SyncConfigurationGuid
				},
				FieldValues = PrepareFields().ToList()
			};

			request.FieldValues.First(x => x.Field.Guid == SyncConfigurationRdo.FieldMappingsGuid).Value = fieldMapping;

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
			configuration.GetFieldValue<string>(SyncConfigurationRdo.FieldMappingsGuid).Should().Be(fieldMapping);
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
					Guid = SyncConfigurationRdo.SyncConfigurationGuid
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
					Guid = SyncConfigurationRdo.CreateSavedSearchInDestinationGuid
				},
				Value = true
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.DataDestinationArtifactIdGuid
				},
				Value = 789456
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.DataDestinationTypeGuid
				},
				Value = "some type"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.DataSourceArtifactIdGuid
				},
				Value = 589632
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.DataSourceTypeGuid
				},
				Value = "source type"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.DestinationFolderStructureBehaviorGuid
				},
				Value = "behavior"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid
				},
				Value = 784512
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.DestinationWorkspaceTagArtifactIdGuid
				},
				Value = 968574
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.EmailNotificationRecipientsGuid
				},
				Value = "emails"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.FieldMappingsGuid
				},
				Value = "fields"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.FieldOverlayBehaviorGuid
				},
				Value = "overlay behavior"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.FolderPathSourceFieldNameGuid
				},
				Value = "folder path"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.ImportOverwriteModeGuid
				},
				Value = "overwrite mode"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.MoveExistingDocumentsGuid
				},
				Value = true
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.NativesBehaviorGuid
				},
				Value = "natives behavior"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.RdoArtifactTypeIdGuid
				},
				Value = 784512
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.SavedSearchInDestinationArtifactIdGuid
				},
				Value = 789632
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.SnapshotIdGuid
				},
				Value = "snapshot id"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.SnapshotRecordsCountGuid
				},
				Value = 813794
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.SourceJobTagArtifactIdGuid
				},
				Value = 951357
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.SourceJobTagNameGuid
				},
				Value = "source tag name"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.SourceWorkspaceTagArtifactIdGuid
				},
				Value = 659823
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SyncConfigurationRdo.SourceWorkspaceTagNameGuid
				},
				Value = "workspace tag name"
			};
			yield return new FieldRefValuePair()
			{
				Field = new FieldRef()
				{
					Guid = SyncConfigurationRdo.ImageImportGuid
				},
				Value = true
			};
			yield return new FieldRefValuePair()
			{
				Field = new FieldRef()
				{
					Guid = SyncConfigurationRdo.IncludeOriginalImagesGuid
				},
				Value = true
			};
			yield return new FieldRefValuePair()
			{
				Field = new FieldRef()
				{
					Guid = SyncConfigurationRdo.ImageFileCopyModeGuid
				},
				Value = "Links"
			};
			yield return new FieldRefValuePair()
			{
				Field = new FieldRef()
				{
					Guid = SyncConfigurationRdo.ProductionImagePrecedenceGuid
				},
				Value = "[2564218,254634]"
			};


#pragma warning restore RG2009 // Hardcoded Numeric Value
		}
	}
}