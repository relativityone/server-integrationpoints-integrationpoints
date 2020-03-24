using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Automation.Utility;
using Relativity.Automation.Utility.Api;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Performance.ARM;
using Relativity.Sync.Tests.Performance.Helpers;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;

namespace Relativity.Sync.Tests.Performance.Tests
{
	public class PerformanceTestBase : SystemTest
	{
		private readonly string _CONTROL_NUMBER_NAME = "Control Number";
		private readonly int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

		public ApiComponent Component { get; }
		public ARMHelper ARMHelper { get; }
		public AzureStorageHelper StorageHelper { get; }

		public WorkspaceRef TargetWorkspace { get; set; }

		public WorkspaceRef SourceWorkspace { get; set; }

		public FullSyncJobConfiguration Configuration { get; set; }

		public PerformanceTestBase()
		{
			RelativityFacade.Instance.RelyOn<ApiComponent>();

			Component = RelativityFacade.Instance.GetComponent<ApiComponent>();

			StorageHelper = AzureStorageHelper.CreateFromTestConfig();

			ARMHelper = ARMHelper.CreateInstance();

			Configuration = new FullSyncJobConfiguration()
			{
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				CreateSavedSearchForTagging = false,
				EmailNotificationRecipients = "",
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings,
				FolderPathSourceFieldName = "Document Folder Path",
				ImportNativeFileCopyMode = ImportNativeFileCopyMode.CopyFiles,
				ImportOverwriteMode = ImportOverwriteMode.AppendOverlay,
				MoveExistingDocuments = false,
			};
		}

		/// <summary>
		///	Creates needed objects in Relativity
		/// </summary>
		/// <returns></returns>
		public async Task SetupConfigurationAsync(int sourceWorkspaceId = 0, int targetWorkspaceId = 0, string savedSearchName = "All Documents",
			IEnumerable<FieldMap> mapping = null, bool useRootWorkspaceFolder = true)
		{
			if (sourceWorkspaceId == 0)
			{
				SourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			}
			else
			{
				SourceWorkspace = await Environment.GetWorkspaceAsync(sourceWorkspaceId).ConfigureAwait(false);
			}

			if (targetWorkspaceId == 0)
			{
				TargetWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			}
			else
			{
				TargetWorkspace = await Environment.GetWorkspaceAsync(targetWorkspaceId).ConfigureAwait(false);
			}

			Configuration.TargetWorkspaceArtifactId = TargetWorkspace.ArtifactID;

			Configuration.SavedSearchArtifactId = await Rdos.GetSavedSearchInstance(ServiceFactory, SourceWorkspace.ArtifactID, savedSearchName).ConfigureAwait(false);


			Configuration.FieldsMapping =
				mapping ?? await GetIdentifierMapping().ConfigureAwait(false);

			Configuration.JobHistoryId =
				await Rdos.CreateJobHistoryInstance(ServiceFactory, SourceWorkspace.ArtifactID).ConfigureAwait(false);

			if (useRootWorkspaceFolder)
			{
				Configuration.DestinationFolderArtifactId =
					await Rdos.GetRootFolderInstance(ServiceFactory, SourceWorkspace.ArtifactID).ConfigureAwait(false);
			}
		}




		private async Task<IEnumerable<FieldMap>> GetIdentifierMapping()
		{
			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest query = PrepareFieldsQueryRequest();
				QueryResult sourceQueryResult = await objectManager.QueryAsync(SourceWorkspace.ArtifactID, query, 0, 1).ConfigureAwait(false);
				QueryResult destinationQueryResult = await objectManager.QueryAsync(TargetWorkspace.ArtifactID, query, 0, 1).ConfigureAwait(false);

				return new FieldMap[]
				{
					new FieldMap
					{
						SourceField = new FieldEntry
						{
							DisplayName = _CONTROL_NUMBER_NAME,
							FieldIdentifier =  sourceQueryResult.Objects.First().ArtifactID,
							IsIdentifier = true
						},
						DestinationField = new FieldEntry
						{
							DisplayName = _CONTROL_NUMBER_NAME,
							FieldIdentifier =  destinationQueryResult.Objects.First().ArtifactID,
							IsIdentifier = true
						},
						FieldMapType = FieldMapType.Identifier
					}
				};

			}
		}

		private QueryRequest PrepareFieldsQueryRequest()
		{
			int fieldArtifactTypeID = (int)ArtifactType.Field;
			QueryRequest queryRequest = new QueryRequest()
			{
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = fieldArtifactTypeID
				},
				Condition = $"'FieldArtifactTypeID' == {_DOCUMENT_ARTIFACT_TYPE_ID} && 'Name'=={_CONTROL_NUMBER_NAME}",
				Fields = null,
				IncludeNameInQueryResult = true
			};

			return queryRequest;
		}
	}
}
