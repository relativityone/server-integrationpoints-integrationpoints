using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.SyncConfiguration;

namespace Relativity.Sync.RDOs
{
	internal class SyncConfigurationRdo
	{
		public int RdoArtifactTypeId { get; set; }
		public string DataSourceType { get; set; }
		public int DataSourceArtifactId { get; set; }

		public int DestinationWorkspaceArtifactId { get; set; }
		public int DataDestinationArtifactId { get; set; }
		public string DataDestinationType { get; set; }
		public string DestinationFolderStructureBehavior { get; set; }
		public string FolderPathSourceFieldName { get; set; }
		public bool CreateSavedSearchInDestination { get; set; }

		public string ImportOverwriteMode { get; set; }
		public string FieldOverlayBehavior { get; set; }

		public string FieldsMapping { get; set; }
		public bool MoveExistingDocuments { get; set; }
		public string NativesBehavior { get; set; }

		public bool ImageImport { get; set; }
		public bool IncludeOriginalImages { get; set; }
		public string ProductionImagePrecedence { get; set; }
		public string ImageFileCopyMode { get; set; }

		public string EmailNotificationRecipients { get; set; }

		public RelativityObject JobHistoryToRetry { get; set; }

		#region GUIDs
		public static readonly Guid SyncConfigurationGuid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57");

		public static readonly Guid CreateSavedSearchInDestinationGuid = new Guid("BFAB4AF6-4704-4A12-A8CA-C96A1FBCB77D");
		public static readonly Guid DataDestinationArtifactIdGuid = new Guid("0E9D7B8E-4643-41CC-9B07-3A66C98248A1");
		public static readonly Guid DataDestinationTypeGuid = new Guid("86D9A34A-B394-41CF-BFF4-BD4FF49A932D");
		public static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		public static readonly Guid DataSourceTypeGuid = new Guid("A00E6BC1-CA1C-48D9-9712-629A63061F0D");
		public static readonly Guid DestinationFolderStructureBehaviorGuid = new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");
		public static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		public static readonly Guid EmailNotificationRecipientsGuid = new Guid("4F03914D-9E86-4B72-B75C-EE48FEEBB583");
		public static readonly Guid FieldMappingsGuid = new Guid("E3CB5C64-C726-47F8-9CB0-1391C5911628");
		public static readonly Guid FieldOverlayBehaviorGuid = new Guid("34ECB263-1370-4D6C-AC11-558447504EC4");
		public static readonly Guid FolderPathSourceFieldNameGuid = new Guid("66A37443-EF92-47ED-BEEA-392464C853D3");
		public static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
		public static readonly Guid MoveExistingDocumentsGuid = new Guid("26F9BF88-420D-4EFF-914B-C47BA36E10BF");
		public static readonly Guid NativesBehaviorGuid = new Guid("D18F0199-7096-4B0C-AB37-4C9A3EA1D3D2");
		public static readonly Guid RdoArtifactTypeIdGuid = new Guid("4DF15F2B-E566-43CE-830D-671BD0786737");

		public static readonly Guid JobHistoryToRetryGuid = new Guid("d7d0ddb9-d383-4578-8d7b-6cbdd9e71549");

		public static readonly Guid ImageImportGuid = new Guid("b282bbe4-7b32-41d1-bb50-960a0e483bb5");
		public static readonly Guid IncludeOriginalImagesGuid = new Guid("f2cad5c5-63d5-49fc-bd47-885661ef1d8b");
		public static readonly Guid ProductionImagePrecedenceGuid = new Guid("421cf05e-bab4-4455-a9ca-fa83d686b5ed");
		public static readonly Guid ImageFileCopyModeGuid = new Guid("bd5dc6d2-faa2-4312-8dc0-4d1b6945dfe1");
		#endregion

		#region Fields Definitions

		public static IDictionary<Guid, BaseFieldRequest> GetFieldsDefinition(int parentObjectTypeId) =>
			new Dictionary<Guid, BaseFieldRequest>
			{
				{
					CreateSavedSearchInDestinationGuid, 
					new YesNoFieldRequest()
					{
						Name = "Create Saved Search in Destination",
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					DataDestinationArtifactIdGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Data Destination Artifact ID",
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					DataDestinationTypeGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Data Destination Type",
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					DataSourceArtifactIdGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Data Source Artifact ID",
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					DataSourceTypeGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Data Source Type",
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					DestinationFolderStructureBehaviorGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Destination Folder Structure Behavior",
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					DestinationWorkspaceArtifactIdGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Destination Workspace Artifact ID",
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					EmailNotificationRecipientsGuid,
					new LongTextFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Email Notification Recipients",
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					FieldMappingsGuid,
					new LongTextFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Field Mappings",
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					FieldOverlayBehaviorGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Field Overlay Behavior",
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					FolderPathSourceFieldNameGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Folder Path Source Field Name",
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					ImportOverwriteModeGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Import Overwrite Mode",
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					MoveExistingDocumentsGuid,
					new YesNoFieldRequest()
					{
						FilterType = FilterType.Boolean,
						Name = "Move Existing Documents",
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					NativesBehaviorGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Natives Behavior",
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					RdoArtifactTypeIdGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "RDO Artifact Type ID",
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					JobHistoryToRetryGuid,
					new SingleObjectFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "JobHistory To Retry",
						AssociativeObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					ImageImportGuid,
					new YesNoFieldRequest()
					{
						FilterType = FilterType.Boolean,
						Name = "Image Import",
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					IncludeOriginalImagesGuid,
					new YesNoFieldRequest()
					{
						FilterType = FilterType.Boolean,
						Name = "Include Original Images",
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					ProductionImagePrecedenceGuid,
					new LongTextFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Production Image Precedence",
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
				{
					ImageFileCopyModeGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = "Image File Copy Mode",
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
					}
				},
			};

		#endregion

		#region EnsureSyncConfigurationExist

		public static async Task<bool> Exists(int workspaceId, ISyncServiceManager servicesMgr)
		{
			using (IArtifactGuidManager artifactGuidManager =
				servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
			{
				return await artifactGuidManager.GuidExistsAsync(workspaceId, SyncConfigurationGuid).ConfigureAwait(false);
			}
		}

		public static async Task<int> CreateType(int workspaceId, int parentObjectId, ISyncServiceManager servicesMgr)
		{
			ObjectTypeRequest objectTypeRequest = new ObjectTypeRequest
			{
				CopyInstancesOnCaseCreation = false,
				CopyInstancesOnParentCopy = false,
				EnableSnapshotAuditingOnDelete = true,
				Keywords = null,
				Name = $"{Guid.NewGuid()}",
				Notes = null,
				ParentObjectType = new Securable<ObjectTypeIdentifier>(new ObjectTypeIdentifier()),
				PersistentListsEnabled = false,
				PivotEnabled = false,
				RelativityApplications = null,
				SamplingEnabled = false,
				UseRelativityForms = null
			};
			
			objectTypeRequest.ParentObjectType.Value.ArtifactTypeID = 
				await ReadObjectTypeId(workspaceId, parentObjectId, servicesMgr).ConfigureAwait(false);

			using (IObjectTypeManager objectTypeManager = servicesMgr.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System))
			using (IArtifactGuidManager guidManager = servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
			{
				int objectTypeArtifactId = await objectTypeManager.CreateAsync(workspaceId, objectTypeRequest).ConfigureAwait(false);

				await guidManager.CreateSingleAsync(workspaceId, objectTypeArtifactId, new List<Guid>() { SyncConfigurationGuid })
					.ConfigureAwait(false);

				using (IFieldManager fieldManager = servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
				{
					foreach (var fieldRequest in GetFieldsDefinition(objectTypeArtifactId))
					{
						int fieldId = await CreateFieldInType(workspaceId, fieldRequest.Value, fieldManager).ConfigureAwait(false);

						await guidManager.CreateSingleAsync(workspaceId, fieldId, new List<Guid>() { fieldRequest.Key }).ConfigureAwait(false);
					}
				}

				return objectTypeArtifactId;
			}
		}

		private static async Task<int> CreateFieldInType(int workspaceId, 
			BaseFieldRequest fieldRequest, IFieldManager fieldManager)
		{
			if (fieldRequest is YesNoFieldRequest yesNoFieldRequest)
			{
				return await fieldManager.CreateYesNoFieldAsync(workspaceId, yesNoFieldRequest).ConfigureAwait(false);
			}
			else if (fieldRequest is WholeNumberFieldRequest wholeNumberFieldRequest)
			{
				return await fieldManager.CreateWholeNumberFieldAsync(workspaceId, wholeNumberFieldRequest).ConfigureAwait(false);
			}
			else if (fieldRequest is FixedLengthFieldRequest fixedLengthFieldRequest)
			{
				return await fieldManager.CreateFixedLengthFieldAsync(workspaceId, fixedLengthFieldRequest).ConfigureAwait(false);
			}
			else if (fieldRequest is LongTextFieldRequest longTextFieldRequest)
			{
				return await fieldManager.CreateLongTextFieldAsync(workspaceId, longTextFieldRequest).ConfigureAwait(false);
			}
			else if (fieldRequest is SingleObjectFieldRequest singleObjectFieldRequest)
			{
				return await fieldManager.CreateSingleObjectFieldAsync(workspaceId, singleObjectFieldRequest).ConfigureAwait(false);
			}
			else
			{
				string typeName = fieldRequest.GetType().ToString();
				throw new NotSupportedException($"Sync doesn't support creation of field type: {typeName}");
			}
		}

		private static async Task<int> ReadObjectTypeId(int workspaceId, int objectId, ISyncServiceManager servicesMgr)
		{
			using (IObjectManager objectManager = servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				ReadRequest request = new ReadRequest()
				{
					Object = new RelativityObjectRef()
					{
						ArtifactID = objectId
					}
				};

				var result = await objectManager.ReadAsync(workspaceId, request).ConfigureAwait(false);

				return result.ObjectType.ArtifactTypeID;
			}
		}

		#endregion
	}
}
