using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Sync.Tests.System.Stubs;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public sealed class ConfigurationRdoTests : SystemTest
	{
		private ISourceServiceFactoryForAdmin _serviceFactory;

		private int _workspaceId;
		private int _jobHistoryArtifactId;

		private static readonly Guid CreateSavedSearchInDestinationGuid = new Guid("BFAB4AF6-4704-4A12-A8CA-C96A1FBCB77D");
		private static readonly Guid DataDestinationArtifactIdGuid = new Guid("0E9D7B8E-4643-41CC-9B07-3A66C98248A1");
		private static readonly Guid DataDestinationTypeGuid = new Guid("86D9A34A-B394-41CF-BFF4-BD4FF49A932D");
		private static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		private static readonly Guid DataSourceTypeGuid = new Guid("A00E6BC1-CA1C-48D9-9712-629A63061F0D");
		private static readonly Guid DestinationFolderStructureBehaviorGuid = new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");
		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid DestinationWorkspaceTagArtifactIdGuid = new Guid("E2100C10-B53B-43FA-BB1B-51E43DCE8208");
		private static readonly Guid EmailNotificationRecipientsGuid = new Guid("4F03914D-9E86-4B72-B75C-EE48FEEBB583");
		private static readonly Guid FieldMappingsGuid = new Guid("E3CB5C64-C726-47F8-9CB0-1391C5911628");
		private static readonly Guid FieldOverlayBehaviorGuid = new Guid("34ECB263-1370-4D6C-AC11-558447504EC4");
		private static readonly Guid FolderPathSourceFieldArtifactIdGuid = new Guid("BF5F07A3-6349-47EE-9618-1DD32C9FD998");
		private static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
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

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);

			_serviceFactory = new SourceServiceFactoryForAdminStub(ServiceFactory);

			WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			_workspaceId = workspace.ArtifactID;

			_jobHistoryArtifactId = await Rdos.CreateJobHistoryInstance(ServiceFactory, _workspaceId).ConfigureAwait(false);
		}

		[Test]
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
					Guid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57")
				},
				FieldValues = PrepareFields().ToList()
			};

			int syncConfigurationArtifactId;
			using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				CreateResult result = await objectManager.CreateAsync(_workspaceId, request).ConfigureAwait(false);
				syncConfigurationArtifactId = result.Object.ArtifactID;
			}

			IConfiguration configuration = await Storage.Configuration
				.GetAsync(_serviceFactory, _workspaceId, syncConfigurationArtifactId, new EmptyLogger(), new SemaphoreSlimWrapper(new SemaphoreSlim(1))).ConfigureAwait(false);

			// ASSERT
			configuration.GetFieldValue<bool>(CreateSavedSearchInDestinationGuid);
			configuration.GetFieldValue<int>(DataDestinationArtifactIdGuid);
			configuration.GetFieldValue<string>(DataDestinationTypeGuid);
			configuration.GetFieldValue<int>(DataSourceArtifactIdGuid);
			configuration.GetFieldValue<string>(DataSourceTypeGuid);
			configuration.GetFieldValue<string>(DestinationFolderStructureBehaviorGuid);
			configuration.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid);
			configuration.GetFieldValue<int>(DestinationWorkspaceTagArtifactIdGuid);
			configuration.GetFieldValue<string>(EmailNotificationRecipientsGuid);
			configuration.GetFieldValue<string>(FieldMappingsGuid);
			configuration.GetFieldValue<string>(FieldOverlayBehaviorGuid);
			configuration.GetFieldValue<int>(FolderPathSourceFieldArtifactIdGuid);
			configuration.GetFieldValue<string>(ImportOverwriteModeGuid);
			configuration.GetFieldValue<RelativityObjectValue>(JobHistoryGuid);
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
		}

		[Test]
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
					Guid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57")
				},
				FieldValues = PrepareFields().ToList()
			};

			using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				await objectManager.CreateAsync(_workspaceId, request).ConfigureAwait(false);
			}
		}

		private IEnumerable<FieldRefValuePair> PrepareFields()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = CreateSavedSearchInDestinationGuid
				},
				Value = true
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = DataDestinationArtifactIdGuid
				},
				Value = 789456
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = DataDestinationTypeGuid
				},
				Value = "some type"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = DataSourceArtifactIdGuid
				},
				Value = 589632
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = DataSourceTypeGuid
				},
				Value = "source type"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = DestinationFolderStructureBehaviorGuid
				},
				Value = "behavior"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = DestinationWorkspaceArtifactIdGuid
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
					Guid = EmailNotificationRecipientsGuid
				},
				Value = "emails"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = FieldMappingsGuid
				},
				Value = "fields"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = FieldOverlayBehaviorGuid
				},
				Value = "overlay behavior"
			};
			yield return new FieldRefValuePair
			{
				Field = new FieldRef
				{
					Guid = FolderPathSourceFieldArtifactIdGuid
				},
				Value = 748521
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

#pragma warning restore RG2009 // Hardcoded Numeric Value
		}
	}
}