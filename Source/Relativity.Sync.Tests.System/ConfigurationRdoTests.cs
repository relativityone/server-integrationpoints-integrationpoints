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

		private static readonly Guid DestinationWorkspaceTagArtifactIdGuid = new Guid("E2100C10-B53B-43FA-BB1B-51E43DCE8208");
		private static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid JobHistoryToRetryGuid = new Guid("d7d0ddb9-d383-4578-8d7b-6cbdd9e71549");
		private static readonly Guid MoveExistingDocumentsGuid = new Guid("26F9BF88-420D-4EFF-914B-C47BA36E10BF");
		private static readonly Guid NativesBehaviorGuid = new Guid("D18F0199-7096-4B0C-AB37-4C9A3EA1D3D2");
		private static readonly Guid RdoArtifactTypeIdGuid = new Guid("4DF15F2B-E566-43CE-830D-671BD0786737");
		private static readonly Guid SavedSearchInDestinationArtifactIdGuid = new Guid("83F4DD7A-2231-4C54-BAAA-D1D5B0FE6E31");
		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");
		private static readonly Guid SourceJobTagArtifactIdGuid = new Guid("C0A63A29-ABAE-4BF4-A3F4-59E5BD87A33E");
		private static readonly Guid SourceJobTagNameGuid = new Guid("DA0E1931-9460-4A61-9033-A8035697C1A4");
		private static readonly Guid SourceWorkspaceTagArtifactIdGuid = new Guid("FEAB129B-AEEF-4AA4-BC91-9EAE9A4C35F6");
		private static readonly Guid SourceWorkspaceTagNameGuid = new Guid("D828B69E-AAAE-4639-91E2-416E35C163B1");

		private static readonly Guid ImageImportGuid = new Guid("b282bbe4-7b32-41d1-bb50-960a0e483bb5");
		private static readonly Guid IncludeOriginalImagesGuid = new Guid("f2cad5c5-63d5-49fc-bd47-885661ef1d8b");
		private static readonly Guid ProductionImagePrecedenceGuid = new Guid("421cf05e-bab4-4455-a9ca-fa83d686b5ed");
		private static readonly Guid ImageFileCopyModeGuid = new Guid("bd5dc6d2-faa2-4312-8dc0-4d1b6945dfe1");

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
			configuration.GetFieldValue<int>(DestinationWorkspaceTagArtifactIdGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.EmailNotificationRecipientsGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.FieldMappingsGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.FieldOverlayBehaviorGuid);
			configuration.GetFieldValue<string>(SyncConfigurationRdo.FolderPathSourceFieldNameGuid);
			configuration.GetFieldValue<string>(ImportOverwriteModeGuid);
			configuration.GetFieldValue<RelativityObjectValue>(JobHistoryGuid);
			configuration.GetFieldValue<RelativityObjectValue>(JobHistoryToRetryGuid);
			configuration.GetFieldValue<bool>(MoveExistingDocumentsGuid);
			configuration.GetFieldValue<string>(NativesBehaviorGuid);
			configuration.GetFieldValue<int>(RdoArtifactTypeIdGuid);
			configuration.GetFieldValue<int>(SavedSearchInDestinationArtifactIdGuid);
			configuration.GetFieldValue<string>(SnapshotIdGuid);
			configuration.GetFieldValue<int>(SnapshotRecordsCountGuid);
			configuration.GetFieldValue<int>(SourceJobTagArtifactIdGuid);
			configuration.GetFieldValue<string>(SourceJobTagNameGuid);
			configuration.GetFieldValue<int>(SourceWorkspaceTagArtifactIdGuid);
			configuration.GetFieldValue<string>(SourceWorkspaceTagNameGuid);

			configuration.GetFieldValue<bool>(ImageImportGuid);
			configuration.GetFieldValue<bool>(IncludeOriginalImagesGuid);
			configuration.GetFieldValue<string>(ImageFileCopyModeGuid);
			configuration.GetFieldValue<string>(ProductionImagePrecedenceGuid);
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
					Guid = DestinationWorkspaceTagArtifactIdGuid
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
					Guid = ImportOverwriteModeGuid
				},
				Value = "overwrite mode"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = MoveExistingDocumentsGuid
				},
				Value = true
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = NativesBehaviorGuid
				},
				Value = "natives behavior"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = RdoArtifactTypeIdGuid
				},
				Value = 784512
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SavedSearchInDestinationArtifactIdGuid
				},
				Value = 789632
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SnapshotIdGuid
				},
				Value = "snapshot id"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SnapshotRecordsCountGuid
				},
				Value = 813794
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SourceJobTagArtifactIdGuid
				},
				Value = 951357
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SourceJobTagNameGuid
				},
				Value = "source tag name"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SourceWorkspaceTagArtifactIdGuid
				},
				Value = 659823
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = SourceWorkspaceTagNameGuid
				},
				Value = "workspace tag name"
			};
			yield return new FieldRefValuePair()
			{
				Field = new FieldRef()
				{
					Guid = ImageImportGuid
				},
				Value = true
			};
			yield return new FieldRefValuePair()
			{
				Field = new FieldRef()
				{
					Guid = IncludeOriginalImagesGuid
				},
				Value = true
			};
			yield return new FieldRefValuePair()
			{
				Field = new FieldRef()
				{
					Guid = ImageFileCopyModeGuid
				},
				Value = "Links"
			};
			yield return new FieldRefValuePair()
			{
				Field = new FieldRef()
				{
					Guid = ProductionImagePrecedenceGuid
				},
				Value = "[2564218,254634]"
			};


#pragma warning restore RG2009 // Hardcoded Numeric Value
		}
	}
}