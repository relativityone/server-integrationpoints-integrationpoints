using System.Collections.Generic;
using System.Linq;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
	internal abstract class SyncConfigurationRootBuilderBase : ISyncConfigurationRootBuilder
	{
		protected readonly ISyncServiceManager ServicesMgr;
		protected readonly ISerializer Serializer;
		protected readonly ISyncContext SyncContext;
		
		public readonly SyncConfigurationRdo SyncConfiguration;

		protected SyncConfigurationRootBuilderBase(ISyncContext syncContext, ISyncServiceManager servicesMgr, ISerializer serializer)
		{
			SyncContext = syncContext;
			ServicesMgr = servicesMgr;
			Serializer = serializer;

			SyncConfiguration = new SyncConfigurationRdo
			{
				DestinationWorkspaceArtifactId = SyncContext.DestinationWorkspaceId,
				ImportOverwriteMode = ImportOverwriteMode.AppendOnly.GetDescription(),
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings.GetDescription()
			};
		}

		public void OverwriteMode(OverwriteOptions options)
		{
			SyncConfiguration.ImportOverwriteMode = options.OverwriteMode.ToString();
			SyncConfiguration.FieldOverlayBehavior = options.FieldsOverlayBehavior.GetDescription();
		}

		public void EmailNotifications(EmailNotificationsOptions options)
		{
			SyncConfiguration.EmailNotificationRecipients = string.Join(
				";", options.Emails.Select(x => x.Trim()));
		}

		public void CreateSavedSearch(CreateSavedSearchOptions options)
		{
			SyncConfiguration.CreateSavedSearchInDestination = options.CreateSavedSearchInDestination;
		}

		public void IsRetry(RetryOptions options)
		{
			SyncConfiguration.JobHistoryToRetry = options.JobToRetry;
		}

		public int Build()
		{
			using (IObjectManager objectManager = ServicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				List<FieldRefValuePair> fields = new List<FieldRefValuePair>();

				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.CreateSavedSearchInDestinationGuid
					},
					Value = SyncConfiguration.CreateSavedSearchInDestination
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.DataDestinationArtifactIdGuid
					},
					Value = SyncConfiguration.DataDestinationArtifactId
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.DataDestinationTypeGuid
					},
					Value = SyncConfiguration.DataDestinationType
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.DataSourceArtifactIdGuid
					},
					Value = SyncConfiguration.DataSourceArtifactId
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.DataSourceTypeGuid
					},
					Value = SyncConfiguration.DataSourceType
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.DestinationFolderStructureBehaviorGuid
					},
					Value = SyncConfiguration.DestinationFolderStructureBehavior
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.FolderPathSourceFieldNameGuid
					},
					Value = SyncConfiguration.FolderPathSourceFieldName
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid
					},
					Value = SyncConfiguration.DestinationWorkspaceArtifactId
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.EmailNotificationRecipientsGuid
					},
					Value = SyncConfiguration.EmailNotificationRecipients
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.FieldMappingsGuid
					},
					Value = SyncConfiguration.FieldsMapping
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.FieldOverlayBehaviorGuid
					},
					Value = SyncConfiguration.FieldOverlayBehavior
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.ImportOverwriteModeGuid
					},
					Value = SyncConfiguration.ImportOverwriteMode
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.MoveExistingDocumentsGuid
					},
					Value = SyncConfiguration.MoveExistingDocuments
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.NativesBehaviorGuid
					},
					Value = SyncConfiguration.NativesBehavior
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.RdoArtifactTypeIdGuid
					},
					Value = SyncConfiguration.RdoArtifactTypeId
				});
				fields.Add(new FieldRefValuePair()
				{
					Field = new FieldRef()
					{
						Guid = SyncConfigurationRdo.ImageImportGuid
					},
					Value = SyncConfiguration.ImageImport
				});
				fields.Add(new FieldRefValuePair()
				{
					Field = new FieldRef()
					{
						Guid = SyncConfigurationRdo.IncludeOriginalImagesGuid
					},
					Value = SyncConfiguration.IncludeOriginalImages
				});
				fields.Add(new FieldRefValuePair()
				{
					Field = new FieldRef()
					{
						Guid = SyncConfigurationRdo.ImageFileCopyModeGuid
					},
					Value = SyncConfiguration.ImageFileCopyMode
				});
				fields.Add(new FieldRefValuePair()
				{
					Field = new FieldRef()
					{
						Guid = SyncConfigurationRdo.ProductionImagePrecedenceGuid
					},
					Value = SyncConfiguration.ProductionImagePrecedence
				});
				fields.Add(new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = SyncConfigurationRdo.JobHistoryToRetryGuid
					},
					Value = SyncConfiguration.JobHistoryToRetry
				});

				var request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = SyncConfigurationRdo.SyncConfigurationGuid
					},
					ParentObject = new RelativityObjectRef
					{
						ArtifactID = SyncContext.ParentObjectId
					},
					FieldValues = fields
				};

				CreateResult result = objectManager.CreateAsync(SyncContext.SourceWorkspaceId, request).GetAwaiter().GetResult();
				return result.Object.ArtifactID;
			}
		}
	}
}
