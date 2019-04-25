using System;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.RelativitySync.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.RelativitySync
{
	public sealed class IntegrationPointToSyncConverter
	{
		private readonly ISerializer _serializer;

		private static readonly Guid CreateSavedSearchInDestinationGuid = new Guid("BFAB4AF6-4704-4A12-A8CA-C96A1FBCB77D");
		private static readonly Guid DataDestinationArtifactIdGuid = new Guid("0E9D7B8E-4643-41CC-9B07-3A66C98248A1");
		private static readonly Guid DataDestinationTypeGuid = new Guid("86D9A34A-B394-41CF-BFF4-BD4FF49A932D");
		private static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		private static readonly Guid DataSourceTypeGuid = new Guid("A00E6BC1-CA1C-48D9-9712-629A63061F0D");
		private static readonly Guid DestinationFolderStructureBehaviorGuid = new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");
		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid EmailNotificationRecipientsGuid = new Guid("4F03914D-9E86-4B72-B75C-EE48FEEBB583");
		private static readonly Guid FieldMappingsGuid = new Guid("E3CB5C64-C726-47F8-9CB0-1391C5911628");
		private static readonly Guid FieldOverlayBehaviorGuid = new Guid("34ECB263-1370-4D6C-AC11-558447504EC4");
		private static readonly Guid FolderPathSourceFieldArtifactIdGuid = new Guid("BF5F07A3-6349-47EE-9618-1DD32C9FD998");
		private static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
		private static readonly Guid MoveExistingDocumentsGuid = new Guid("26F9BF88-420D-4EFF-914B-C47BA36E10BF");
		private static readonly Guid NativesBehaviorGuid = new Guid("D18F0199-7096-4B0C-AB37-4C9A3EA1D3D2");
		private static readonly Guid RdoArtifactTypeIdGuid = new Guid("4DF15F2B-E566-43CE-830D-671BD0786737");

		public IntegrationPointToSyncConverter(ISerializer serializer)
		{
			_serializer = serializer;
		}

		public async Task<int> CreateSyncConfiguration(IExtendedJob job, IHelper helper)
		{
			using (IObjectManager objectManager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(job.IntegrationPointModel.SourceConfiguration);
				ImportSettings importSettings = _serializer.Deserialize<ImportSettings>(job.IntegrationPointModel.DestinationConfiguration);
				FolderConf folderConf = _serializer.Deserialize<FolderConf>(job.IntegrationPointModel.DestinationConfiguration);
				CreateRequest request = PrepareCreateRequest(job, sourceConfiguration, importSettings, folderConf);
				CreateResult result = await objectManager.CreateAsync(job.WorkspaceId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		private string DestinationFolderStructureBehavior(FolderConf folderConf)
		{
			if (!folderConf.UseDynamicFolderPath && !folderConf.UseFolderPathInformation)
			{
				return "None";
			}

			if (folderConf.UseDynamicFolderPath)
			{
				return "RetainSourceWorkspaceStructure";
			}

			return "ReadFromField";
		}

		private string NativesBehavior(ImportNativeFileCopyModeEnum mode)
		{
			if (mode == ImportNativeFileCopyModeEnum.CopyFiles)
			{
				return "Copy";
			}

			if (mode == ImportNativeFileCopyModeEnum.SetFileLinks)
			{
				return "Link";
			}

			return "None";
		}

		private CreateRequest PrepareCreateRequest(IExtendedJob job, SourceConfiguration sourceConfiguration, ImportSettings importSettings, FolderConf folderConf)
		{
			return new CreateRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57")
				},
				ParentObject = new RelativityObjectRef
				{
					ArtifactID = job.JobHistoryId
				},
				FieldValues = new[]
				{
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = CreateSavedSearchInDestinationGuid
						},
						Value = importSettings.CreateSavedSearchForTagging
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = DataDestinationArtifactIdGuid
						},
						Value = importSettings.DestinationFolderArtifactId
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = DataDestinationTypeGuid
						},
						Value = "Folder"
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = DataSourceArtifactIdGuid
						},
						Value = sourceConfiguration.SavedSearchArtifactId
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = DataSourceTypeGuid
						},
						Value = "SavedSearch"
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = DestinationFolderStructureBehaviorGuid
						},
						Value = DestinationFolderStructureBehavior(folderConf)
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = FolderPathSourceFieldArtifactIdGuid
						},
						Value = folderConf.FolderPathSourceField
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = DestinationWorkspaceArtifactIdGuid
						},
						Value = sourceConfiguration.TargetWorkspaceArtifactId
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = EmailNotificationRecipientsGuid
						},
						Value = job.IntegrationPointModel.EmailNotificationRecipients
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = FieldMappingsGuid
						},
						Value = job.IntegrationPointModel.FieldMappings
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = FieldOverlayBehaviorGuid
						},
						Value = importSettings.FieldOverlayBehavior
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = ImportOverwriteModeGuid
						},
						Value = importSettings.ImportOverwriteMode.ToString()
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = MoveExistingDocumentsGuid
						},
						Value = importSettings.MoveExistingDocuments
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = NativesBehaviorGuid
						},
						Value = NativesBehavior(importSettings.ImportNativeFileCopyMode)
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = RdoArtifactTypeIdGuid
						},
						Value = 10
					}
				}
			};
		}
	}
}