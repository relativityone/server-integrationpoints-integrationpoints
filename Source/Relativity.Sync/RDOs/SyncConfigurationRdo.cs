using System;
using System.Collections.Generic;
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
		public int SavedSearchInDestinationArtifactId { get; set; }
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


		public string SnapshotId { get; set; }
		public int SnapshotRecordsCount { get; set; }
		public int SourceJobTagArtifactId { get; set; }
		public string SourceJobTagName { get; set; }
		public int SourceWorkspaceTagArtifactId { get; set; }
		public string SourceWorkspaceTagName { get; set; }
		public int DestinationWorkspaceTagArtifactId { get; set; }

		#region GUIDs
		public static readonly Guid SyncConfigurationGuid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57");

		public static readonly Guid CreateSavedSearchInDestinationGuid = new Guid("BFAB4AF6-4704-4A12-A8CA-C96A1FBCB77D");
		public static readonly Guid SavedSearchInDestinationArtifactIdGuid = new Guid("83F4DD7A-2231-4C54-BAAA-D1D5B0FE6E31");
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
		public static readonly Guid JobHistoryToRetryGuid = new Guid("D7D0DDB9-D383-4578-8D7B-6CBDD9E71549");
		public static readonly Guid ImageImportGuid = new Guid("B282BBE4-7B32-41D1-BB50-960A0E483BB5");
		public static readonly Guid IncludeOriginalImagesGuid = new Guid("F2CAD5C5-63D5-49FC-BD47-885661EF1D8B");
		public static readonly Guid ProductionImagePrecedenceGuid = new Guid("421CF05E-BAB4-4455-A9CA-FA83D686B5ED");
		public static readonly Guid ImageFileCopyModeGuid = new Guid("BD5DC6D2-FAA2-4312-8DC0-4D1B6945DFE1");

		public static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		public static readonly Guid SnapshotRecordsCountGuid = new Guid("57B93F20-2648-4ACF-973B-BCBA8A08E2BD");
		public static readonly Guid SourceJobTagArtifactIdGuid = new Guid("C0A63A29-ABAE-4BF4-A3F4-59E5BD87A33E");
		public static readonly Guid SourceJobTagNameGuid = new Guid("DA0E1931-9460-4A61-9033-A8035697C1A4");
		public static readonly Guid SourceWorkspaceTagArtifactIdGuid = new Guid("FEAB129B-AEEF-4AA4-BC91-9EAE9A4C35F6");
		public static readonly Guid SourceWorkspaceTagNameGuid = new Guid("D828B69E-AAAE-4639-91E2-416E35C163B1");
		public static readonly Guid DestinationWorkspaceTagArtifactIdGuid = new Guid("E2100C10-B53B-43FA-BB1B-51E43DCE8208");

		public static readonly IDictionary<Guid, string> GuidNames = new Dictionary<Guid, string>
		{
			{CreateSavedSearchInDestinationGuid, "Create Saved Search in Destination"},
			{SavedSearchInDestinationArtifactIdGuid, "Saved Search In Destination Artifact ID"},
			{DataDestinationArtifactIdGuid, "Data Destination Artifact ID"},
			{DataDestinationTypeGuid, "Data Destination Type"},
			{DataSourceArtifactIdGuid, "Data Source Artifact ID"},
			{DataSourceTypeGuid, "Data Source Type"},
			{DestinationFolderStructureBehaviorGuid, "Destination Folder Structure Behavior"},
			{DestinationWorkspaceArtifactIdGuid, "Destination Workspace Artifact ID"},
			{EmailNotificationRecipientsGuid, "Email Notification Recipients"},
			{FieldMappingsGuid, "Field Mappings"},
			{FieldOverlayBehaviorGuid, "Field Overlay Behavior"},
			{FolderPathSourceFieldNameGuid, "Folder Path Source Field Name"},
			{ImportOverwriteModeGuid, "Import Overwrite Mode"},
			{MoveExistingDocumentsGuid, "Move Existing Documents"},
			{NativesBehaviorGuid, "Natives Behavior"},
			{RdoArtifactTypeIdGuid, "RDO Artifact Type ID"},
			{JobHistoryToRetryGuid, "JobHistory To Retry"},
			{ImageImportGuid, "Image Import"},
			{IncludeOriginalImagesGuid, "Include Original Images"},
			{ProductionImagePrecedenceGuid, "Production Image Precedence"},
			{ImageFileCopyModeGuid, "Image File Copy Mode"},
			
			{SnapshotIdGuid, "Snapshot ID"},
			{SnapshotRecordsCountGuid , "Snapshot Records Count"},
			{SourceJobTagArtifactIdGuid, "Source Job Tag Artifact ID"},
			{SourceJobTagNameGuid, "Source Job Tag Name"},
			{SourceWorkspaceTagArtifactIdGuid, "Source Workspace Tag Artifact ID"},
			{SourceWorkspaceTagNameGuid, "Source Workspace Tag Name"},
			{DestinationWorkspaceTagArtifactIdGuid, "Destination Workspace Tag Artifact ID"},
		};

		#endregion

		#region Fields Definitions

		public static ObjectTypeRequest GetObjectTypeDefinition(int parentObjectTypeId)
		{
			var objectTypeRequest = new ObjectTypeRequest
			{
				CopyInstancesOnCaseCreation = false,
				CopyInstancesOnParentCopy = false,
				EnableSnapshotAuditingOnDelete = true,
				Keywords = null,
				Name = "Sync Configuration",
				Notes = null,
				ParentObjectType = new Securable<ObjectTypeIdentifier>(new ObjectTypeIdentifier()),
				PersistentListsEnabled = false,
				PivotEnabled = false,
				RelativityApplications = null,
				SamplingEnabled = false,
				UseRelativityForms = null
			};

			objectTypeRequest.ParentObjectType.Value.ArtifactTypeID = parentObjectTypeId;

			return objectTypeRequest;
		}

		public static IDictionary<Guid, BaseFieldRequest> GetFieldsDefinition(int objectTypeId, int parentObjectTypeId) =>
			new Dictionary<Guid, BaseFieldRequest>
			{
				{
					CreateSavedSearchInDestinationGuid, 
					new YesNoFieldRequest()
					{
						Name = GuidNames[CreateSavedSearchInDestinationGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					SavedSearchInDestinationArtifactIdGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[SavedSearchInDestinationArtifactIdGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					DataDestinationArtifactIdGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[DataDestinationArtifactIdGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					DataDestinationTypeGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[DataDestinationTypeGuid],
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					DataSourceArtifactIdGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[DataSourceArtifactIdGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					DataSourceTypeGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[DataSourceTypeGuid],
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					DestinationFolderStructureBehaviorGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[DestinationFolderStructureBehaviorGuid],
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					DestinationWorkspaceArtifactIdGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[DestinationWorkspaceArtifactIdGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					EmailNotificationRecipientsGuid,
					new LongTextFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[EmailNotificationRecipientsGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					FieldMappingsGuid,
					new LongTextFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[FieldMappingsGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					FieldOverlayBehaviorGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[FieldOverlayBehaviorGuid],
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					FolderPathSourceFieldNameGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[FolderPathSourceFieldNameGuid],
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					ImportOverwriteModeGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[ImportOverwriteModeGuid],
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					MoveExistingDocumentsGuid,
					new YesNoFieldRequest()
					{
						FilterType = FilterType.Boolean,
						Name = GuidNames[MoveExistingDocumentsGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					NativesBehaviorGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[NativesBehaviorGuid],
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					RdoArtifactTypeIdGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[RdoArtifactTypeIdGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					JobHistoryToRetryGuid,
					new SingleObjectFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[JobHistoryToRetryGuid],
						AssociativeObjectType = new ObjectTypeIdentifier { ArtifactID = parentObjectTypeId },
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					ImageImportGuid,
					new YesNoFieldRequest()
					{
						FilterType = FilterType.Boolean,
						Name = GuidNames[ImageImportGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					IncludeOriginalImagesGuid,
					new YesNoFieldRequest()
					{
						FilterType = FilterType.Boolean,
						Name = GuidNames[IncludeOriginalImagesGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					ProductionImagePrecedenceGuid,
					new LongTextFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[ProductionImagePrecedenceGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					ImageFileCopyModeGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[ImageFileCopyModeGuid],
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					SnapshotIdGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[SnapshotIdGuid],
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					SnapshotRecordsCountGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[SnapshotRecordsCountGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					SourceJobTagArtifactIdGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[SourceJobTagArtifactIdGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					SourceJobTagNameGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[SourceJobTagNameGuid],
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					SourceWorkspaceTagArtifactIdGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[SourceWorkspaceTagArtifactIdGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					SourceWorkspaceTagNameGuid,
					new FixedLengthFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[SourceWorkspaceTagNameGuid],
						Length = 255,
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
				{
					DestinationWorkspaceTagArtifactIdGuid,
					new WholeNumberFieldRequest()
					{
						FilterType = FilterType.TextBox,
						Name = GuidNames[DestinationWorkspaceTagArtifactIdGuid],
						ObjectType = new ObjectTypeIdentifier { ArtifactID = objectTypeId },
					}
				},
			};

		#endregion

		#region Sync Configuration RDO Methods

		public async Task<int> SaveAsync(int workspaceId, int parentObjectId, ISyncServiceManager servicesMgr)
		{
			using (IObjectManager objectManager = servicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				List<FieldRefValuePair> fields = new List<FieldRefValuePair>();

				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = CreateSavedSearchInDestinationGuid
					},
					Value = CreateSavedSearchInDestination
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SavedSearchInDestinationArtifactIdGuid
					},
					Value = SavedSearchInDestinationArtifactId
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = DataDestinationArtifactIdGuid
					},
					Value = DataDestinationArtifactId
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = DataDestinationTypeGuid
					},
					Value = DataDestinationType
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = DataSourceArtifactIdGuid
					},
					Value = DataSourceArtifactId
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = DataSourceTypeGuid
					},
					Value = DataSourceType
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = DestinationFolderStructureBehaviorGuid
					},
					Value = DestinationFolderStructureBehavior
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = FolderPathSourceFieldNameGuid
					},
					Value = FolderPathSourceFieldName
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = DestinationWorkspaceArtifactIdGuid
					},
					Value = DestinationWorkspaceArtifactId
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = EmailNotificationRecipientsGuid
					},
					Value = EmailNotificationRecipients
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = FieldMappingsGuid
					},
					Value = FieldsMapping
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = FieldOverlayBehaviorGuid
					},
					Value = FieldOverlayBehavior
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = ImportOverwriteModeGuid
					},
					Value = ImportOverwriteMode
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = MoveExistingDocumentsGuid
					},
					Value = MoveExistingDocuments
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = NativesBehaviorGuid
					},
					Value = NativesBehavior
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = RdoArtifactTypeIdGuid
					},
					Value = RdoArtifactTypeId
				});
				fields.Add(new FieldRefValuePair()
				{
					Field = new FieldRef()
					{
						Guid = ImageImportGuid
					},
					Value = ImageImport
				});
				fields.Add(new FieldRefValuePair()
				{
					Field = new FieldRef()
					{
						Guid = IncludeOriginalImagesGuid
					},
					Value = IncludeOriginalImages
				});
				fields.Add(new FieldRefValuePair()
				{
					Field = new FieldRef()
					{
						Guid = ImageFileCopyModeGuid
					},
					Value = ImageFileCopyMode
				});
				fields.Add(new FieldRefValuePair()
				{
					Field = new FieldRef()
					{
						Guid = ProductionImagePrecedenceGuid
					},
					Value = ProductionImagePrecedence
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = JobHistoryToRetryGuid
					},
					Value = JobHistoryToRetry
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SnapshotIdGuid
					},
					Value = SnapshotId
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SnapshotRecordsCountGuid
					},
					Value = SnapshotRecordsCount
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SourceJobTagArtifactIdGuid
					},
					Value = SourceJobTagArtifactId
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SourceJobTagNameGuid
					},
					Value = SourceJobTagName
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SourceWorkspaceTagArtifactIdGuid
					},
					Value = SourceWorkspaceTagArtifactId
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SourceWorkspaceTagNameGuid
					},
					Value = SourceWorkspaceTagName
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = DestinationWorkspaceTagArtifactIdGuid
					},
					Value = DestinationWorkspaceTagArtifactId
				});

				var request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = SyncConfigurationGuid
					},
					ParentObject = new RelativityObjectRef
					{
						ArtifactID = parentObjectId
					},
					FieldValues = fields
				};

				CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		public static async Task<bool> ExistsAsync(int workspaceId, ISyncServiceManager servicesMgr)
		{
			using (IArtifactGuidManager artifactGuidManager =
				servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
			{
				return await artifactGuidManager.GuidExistsAsync(workspaceId, SyncConfigurationGuid).ConfigureAwait(false);
			}
		}

		public static async Task<int> CreateTypeAsync(int workspaceId, int parentObjectId, ISyncServiceManager servicesMgr)
		{
			using (IObjectTypeManager objectTypeManager = servicesMgr.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System))
			using (IArtifactGuidManager guidManager = servicesMgr.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
			{
				var parentObject = await ReadObjectAsync(workspaceId, parentObjectId, servicesMgr).ConfigureAwait(false);
				
				var objectTypeRequest = GetObjectTypeDefinition(parentObject.ObjectType.ArtifactTypeID);

				int objectTypeArtifactId = await objectTypeManager.CreateAsync(workspaceId, objectTypeRequest).ConfigureAwait(false);

				await guidManager.CreateSingleAsync(workspaceId, objectTypeArtifactId, new List<Guid>() { SyncConfigurationGuid })
					.ConfigureAwait(false);

				using (IFieldManager fieldManager = servicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
				{
					foreach (var fieldRequest in GetFieldsDefinition(objectTypeArtifactId, parentObject.ObjectType.ArtifactID))
					{
						int fieldId = await CreateFieldInTypeAsync(workspaceId, fieldRequest.Value, fieldManager).ConfigureAwait(false);

						await guidManager.CreateSingleAsync(workspaceId, fieldId, new List<Guid>() { fieldRequest.Key }).ConfigureAwait(false);
					}
				}

				return objectTypeArtifactId;
			}
		}

		private static async Task<int> CreateFieldInTypeAsync(int workspaceId, 
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

		private static async Task<ReadResult> ReadObjectAsync(int workspaceId, int objectId, ISyncServiceManager servicesMgr)
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

				return await objectManager.ReadAsync(workspaceId, request).ConfigureAwait(false);
			}
		}

		#endregion
	}
}
