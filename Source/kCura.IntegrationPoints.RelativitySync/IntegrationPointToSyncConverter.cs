using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.RelativitySync.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.RelativitySync
{
	public sealed class IntegrationPointToSyncConverter
	{
		private readonly ISerializer _serializer;
		private readonly IJobHistoryService _jobHistoryService;

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
		private static readonly Guid FolderPathSourceFieldNameGuid = new Guid("66A37443-EF92-47ED-BEEA-392464C853D3");
		private static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
		private static readonly Guid MoveExistingDocumentsGuid = new Guid("26F9BF88-420D-4EFF-914B-C47BA36E10BF");
		private static readonly Guid NativesBehaviorGuid = new Guid("D18F0199-7096-4B0C-AB37-4C9A3EA1D3D2");
		private static readonly Guid RdoArtifactTypeIdGuid = new Guid("4DF15F2B-E566-43CE-830D-671BD0786737");

		private static readonly Guid JobHistoryToRetryGuid = new Guid("d7d0ddb9-d383-4578-8d7b-6cbdd9e71549");

		private static readonly Guid ImageImportGuid = new Guid("b282bbe4-7b32-41d1-bb50-960a0e483bb5");
		private static readonly Guid IncludeOriginalImagesGuid = new Guid("f2cad5c5-63d5-49fc-bd47-885661ef1d8b");
		private static readonly Guid ProductionImagePrecedenceGuid = new Guid("421cf05e-bab4-4455-a9ca-fa83d686b5ed");
		private static readonly Guid ImageFileCopyModeGuid = new Guid("bd5dc6d2-faa2-4312-8dc0-4d1b6945dfe1");

		public IntegrationPointToSyncConverter(ISerializer serializer, IJobHistoryService jobHistoryService)
		{
			_serializer = serializer;
			_jobHistoryService = jobHistoryService;
		}

		public async Task<int> CreateSyncConfigurationAsync(IExtendedJob job, IHelper helper)
		{
			using (IObjectManager objectManager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
			{
				SourceConfiguration sourceConfiguration = _serializer.Deserialize<SourceConfiguration>(job.IntegrationPointModel.SourceConfiguration);
				ImportSettings importSettings = _serializer.Deserialize<ImportSettings>(job.IntegrationPointModel.DestinationConfiguration);
				FolderConf folderConf = _serializer.Deserialize<FolderConf>(job.IntegrationPointModel.DestinationConfiguration);

				RelativityObject jobHistoryToRetry = null;
				if (IsRetryingErrors(job.Job))
				{
					jobHistoryToRetry = await JobHistoryHelper.GetLastJobHistoryWithErrorsAsync(sourceConfiguration.SourceWorkspaceArtifactId, job.IntegrationPointId, helper).ConfigureAwait(false);
				}
				
				CreateRequest request = await PrepareCreateRequestAsync(job, sourceConfiguration, importSettings, folderConf, objectManager, jobHistoryToRetry).ConfigureAwait(false);
				CreateResult result = await objectManager.CreateAsync(job.WorkspaceId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		private bool IsRetryingErrors(Job job)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			JobHistory jobHistory = _jobHistoryService.GetRdo(taskParameters.BatchInstance);

			if (jobHistory == null)
			{
				// this means that job is scheduled, so it's not retrying errors 
				return false;
			}

			return jobHistory.JobType.EqualsToChoice(JobTypeChoices.JobHistoryRetryErrors);
		}

		private static string DestinationFolderStructureBehavior(FolderConf folderConf)
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

		private static string CopyNativeFilesBehavior(ImportNativeFileCopyModeEnum mode)
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

		private static string CopyImageFilesBehavior(ImportNativeFileCopyModeEnum mode)
		{
			if (mode == ImportNativeFileCopyModeEnum.CopyFiles)
			{
				return "Copy";
			}
			else
			{
				return "Link";
			}
		}

		private static async Task<string> GetFolderPathSourceFieldNameAsync(int artifactId, int workspaceId, IObjectManager objectManager)
		{
			if (artifactId == 0)
			{
				return string.Empty;
			}

			var request = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef {Name = "Field"},
				Condition = $"\"ArtifactID\" == {artifactId}",
				Fields = new[] {new FieldRef {Name = "Name"}}
			};
			QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceId, request, 0, 1).ConfigureAwait(false);
			return result.Objects[0].Values[0].ToString();
		}
		
		private string GetProductionImagePrecedence(ImportSettings importSettings)
		{
			return _serializer.Serialize(importSettings.ImagePrecedence.Select(x => int.Parse(x.ArtifactID)));
		}

		private async Task<CreateRequest> PrepareCreateRequestAsync(IExtendedJob job, 
			SourceConfiguration sourceConfiguration,
			ImportSettings importSettings, 
			FolderConf folderConf, 
			IObjectManager objectManager,
			RelativityObject jobHistoryToRetry)
		{
			string folderPathSourceFieldName = await GetFolderPathSourceFieldNameAsync(folderConf.FolderPathSourceField, sourceConfiguration.SourceWorkspaceArtifactId, objectManager).ConfigureAwait(false);

			List<FieldRefValuePair> fields = new List<FieldRefValuePair>()
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
						Guid = FolderPathSourceFieldNameGuid
					},
					Value = folderPathSourceFieldName
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
					Value = FieldMapHelper.FixMappings(job.IntegrationPointModel.FieldMappings, _serializer)
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
					Value = CopyNativeFilesBehavior(importSettings.ImportNativeFileCopyMode)
				},
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = RdoArtifactTypeIdGuid
					},
					Value = 10
				},
				new FieldRefValuePair
				{
					Field = new FieldRef
					{
						Guid = JobHistoryToRetryGuid
					},
					Value = jobHistoryToRetry
				},
				new FieldRefValuePair()
				{
					Field = new FieldRef()
					{
						Guid = ImageImportGuid
					},
					Value = importSettings.ImageImport
				}
			};

			if (importSettings.ImageImport)
			{
				fields.AddRange(new[]
				{
					new FieldRefValuePair()
					{
						Field = new FieldRef()
						{
							Guid = IncludeOriginalImagesGuid
						},
						Value = importSettings.IncludeOriginalImages
					},
					new FieldRefValuePair()
					{
						Field = new FieldRef()
						{
							Guid = ImageFileCopyModeGuid
						},
						Value = CopyImageFilesBehavior(importSettings.ImportNativeFileCopyMode)
					},
					new FieldRefValuePair()
					{
						Field = new FieldRef()
						{
							Guid = ProductionImagePrecedenceGuid
						},
						Value = GetProductionImagePrecedence(importSettings)
					}
				});
			}

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
				FieldValues = fields
			};
		}
	}
}