using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Sync.Utils;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.System.Core.Runner;

namespace Relativity.Sync.Tests.System.Core.Helpers
{
	internal static class Rdos
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = (int)ArtifactType.Document;

		private static readonly Guid _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_GUID =
			new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");

		private static readonly Guid _RELATIVITY_SOURCE_JOB_OBJECT_TYPE_GUID =
			new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");

		private static readonly Guid _DESTINATION_WORKSPACE_OBJECT_TYPE_GUID =
			new Guid("3F45E490-B4CF-4C7D-8BB6-9CA891C0C198");

		private static readonly Guid _RELATIVITY_SOURCE_CASE_ID_FIELD_GUID =
			new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");

		private static readonly Guid _RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID =
			new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");

		private static readonly Guid _RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID =
			new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");

		private static readonly Guid _RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_GUID =
			new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");

		private static readonly Guid _RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_GUID =
			new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");

		private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_ARTIFACTID_GUID =
			new Guid("323458db-8a06-464b-9402-af2516cf47e0");

		private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_NAME_GUID =
			new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554");

		private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_ARTIFACTID_GUID =
			new Guid("207e6836-2961-466b-a0d2-29974a4fad36");

		private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_NAME_GUID =
			new Guid("348d7394-2658-4da4-87d0-8183824adf98");

		private static readonly Guid _DESTINATION_WORKSPACE_NAME_GUID =
			new Guid("155649c0-db15-4ee7-b449-bfdf2a54b7b5");

		private static readonly Guid _SYNC_CONFIGURATION_FIELD_MAPPINGS_GUID =
			new Guid("E3CB5C64-C726-47F8-9CB0-1391C5911628");

		private static readonly Guid CreateSavedSearchInDestinationGuid =
			new Guid("BFAB4AF6-4704-4A12-A8CA-C96A1FBCB77D");

		private static readonly Guid DataDestinationArtifactIdGuid = new Guid("0E9D7B8E-4643-41CC-9B07-3A66C98248A1");
		private static readonly Guid DataDestinationTypeGuid = new Guid("86D9A34A-B394-41CF-BFF4-BD4FF49A932D");
		private static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		private static readonly Guid DataSourceTypeGuid = new Guid("A00E6BC1-CA1C-48D9-9712-629A63061F0D");

		private static readonly Guid DestinationFolderStructureBehaviorGuid =
			new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");

		private static readonly Guid DestinationWorkspaceArtifactIdGuid =
			new Guid("15B88438-6CF7-47AB-B630-424633159C69");

		private static readonly Guid EmailNotificationRecipientsGuid = new Guid("4F03914D-9E86-4B72-B75C-EE48FEEBB583");
		private static readonly Guid FieldMappingsGuid = new Guid("E3CB5C64-C726-47F8-9CB0-1391C5911628");
		private static readonly Guid FieldOverlayBehaviorGuid = new Guid("34ECB263-1370-4D6C-AC11-558447504EC4");
		private static readonly Guid FolderPathSourceFieldNameGuid = new Guid("66A37443-EF92-47ED-BEEA-392464C853D3");
		private static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
		private static readonly Guid MoveExistingDocumentsGuid = new Guid("26F9BF88-420D-4EFF-914B-C47BA36E10BF");
		private static readonly Guid NativesBehaviorGuid = new Guid("D18F0199-7096-4B0C-AB37-4C9A3EA1D3D2");
		private static readonly Guid RdoArtifactTypeIdGuid = new Guid("4DF15F2B-E566-43CE-830D-671BD0786737");

		public static async Task<int> CreateJobHistoryInstance(ServiceFactory serviceFactory, int workspaceId,
			string name = "Name")
		{
			using (var objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				CreateRequest request = new CreateRequest
				{
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Value = name,
							Field = new FieldRef
							{
								Name = "Name"
							}
						}
					},
					ObjectType = new ObjectTypeRef
					{
						Guid = new Guid("08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9")
					}
				};
				CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		public static async Task<int> CreateSyncConfigurationInstance(ServiceFactory serviceFactory, int workspaceId,
			int jobHistoryId, List<FieldMap> fieldMappings = null)
		{
			using (IObjectManager objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				CreateRequest request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57")
					},
					ParentObject = new RelativityObjectRef
					{
						ArtifactID = jobHistoryId
					},
					FieldValues = new List<FieldRefValuePair>
					{
						new FieldRefValuePair
						{
							Field = new FieldRef {Guid = _SYNC_CONFIGURATION_FIELD_MAPPINGS_GUID},
							Value = new JSONSerializer().Serialize(fieldMappings)
						}
					}
				};
				CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		public static async Task<int> CreateRelativitySourceCaseInstance(ServiceFactory serviceFactory,
			int destinationWorkspaceArtifactId, RelativitySourceCaseTag tag)
		{
			using (var objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				CreateRequest request = new CreateRequest
				{
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Field = new FieldRef {Name = "Name"},
							Value = tag.Name
						},
						new FieldRefValuePair
						{
							Field = new FieldRef {Guid = _RELATIVITY_SOURCE_CASE_ID_FIELD_GUID},
							Value = tag.SourceWorkspaceArtifactId
						},
						new FieldRefValuePair
						{
							Field = new FieldRef {Guid = _RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID},
							Value = tag.SourceWorkspaceName
						},
						new FieldRefValuePair
						{
							Field = new FieldRef {Guid = _RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID},
							Value = tag.SourceInstanceName
						}
					},
					ObjectType = new ObjectTypeRef
					{
						Guid = _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_GUID
					}
				};
				CreateResult result = await objectManager.CreateAsync(destinationWorkspaceArtifactId, request)
					.ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		public static async Task<int> CreateRelativitySourceJobInstance(ServiceFactory serviceFactory,
			int destinationWorkspaceArtifactId, RelativitySourceJobTag tag)
		{
			using (var objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				CreateRequest request = new CreateRequest
				{
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Field = new FieldRef {Name = "Name"},
							Value = tag.Name
						},
						new FieldRefValuePair
						{
							Field = new FieldRef {Guid = _RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_GUID},
							Value = tag.JobHistoryArtifactId
						},
						new FieldRefValuePair
						{
							Field = new FieldRef {Guid = _RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_GUID},
							Value = tag.JobHistoryName
						},
					},
					ParentObject = new RelativityObjectRef { ArtifactID = tag.SourceCaseTagArtifactId },
					ObjectType = new ObjectTypeRef
					{
						Guid = _RELATIVITY_SOURCE_JOB_OBJECT_TYPE_GUID
					}
				};
				CreateResult result = await objectManager.CreateAsync(destinationWorkspaceArtifactId, request)
					.ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		public static async Task<int> CreateDestinationWorkspaceTagInstance(ServiceFactory serviceFactory,
			int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName)
		{
			const string destinationInstanceName = "This Instance";
			string tagName = $"Test {destinationWorkspaceName}";

			using (var objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				CreateRequest request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = _DESTINATION_WORKSPACE_OBJECT_TYPE_GUID
					},
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Field = new FieldRef {Guid = _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_ARTIFACTID_GUID},
							Value = destinationWorkspaceArtifactId
						},
						new FieldRefValuePair
						{
							Field = new FieldRef {Guid = _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_ARTIFACTID_GUID},
							Value = -1
						},
						new FieldRefValuePair
						{
							Field = new FieldRef {Guid = _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_NAME_GUID},
							Value = destinationInstanceName
						},
						new FieldRefValuePair
						{
							Field = new FieldRef {Guid = _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_NAME_GUID},
							Value = destinationWorkspaceName
						},
						new FieldRefValuePair
						{
							Field = new FieldRef {Guid = _DESTINATION_WORKSPACE_NAME_GUID},
							Value = tagName
						}
					}
				};

				CreateResult result = await objectManager.CreateAsync(sourceWorkspaceArtifactId, request)
					.ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		public static async Task<int> GetSavedSearchInstance(ServiceFactory serviceFactory, int workspaceId,
			string name = "All Documents")
		{
			using (IKeywordSearchManager keywordSearchManager = serviceFactory.CreateProxy<IKeywordSearchManager>())
			{
				Services.Query request = new Services.Query
				{
					Condition = $"(('Name' == '{name}'))"
				};
				KeywordSearchQueryResultSet result =
					await keywordSearchManager.QueryAsync(workspaceId, request).ConfigureAwait(false);
				if (result.TotalCount == 0)
				{
					throw new InvalidOperationException(
						$"Cannot find saved search '{name}' in workspace {workspaceId}");
				}

				return result.Results.First().Artifact.ArtifactID;
			}
		}

		public static async Task<int> GetRootFolderInstance(ServiceFactory serviceFactory, int workspaceId)
		{
			using (IFolderManager folderManager = serviceFactory.CreateProxy<IFolderManager>())
			{
				Folder rootFolder = await folderManager.GetWorkspaceRootAsync(workspaceId).ConfigureAwait(false);
				return rootFolder.ArtifactID;
			}
		}

		public static async Task<int> CreateFolderInstance(ServiceFactory serviceFactory, int workspaceId,
			string folderName)
		{
			using (IFolderManager folderManager = serviceFactory.CreateProxy<IFolderManager>())
			{
				Folder rootFolder = await folderManager.GetWorkspaceRootAsync(workspaceId).ConfigureAwait(false);
				var folder = new Folder
				{
					ParentFolder = new FolderRef(rootFolder.ArtifactID),
					Name = folderName
				};
				int childFolderArtifactId =
					await folderManager.CreateSingleAsync(workspaceId, folder).ConfigureAwait(false);
				return childFolderArtifactId;
			}
		}

		public static async Task<string> GetFolderPathSourceFieldName(ServiceFactory serviceFactory, int workspaceId)
		{
			using (IObjectManager objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Name = "Field"
					},
					Condition = "(('Name' == 'Document Folder Path'))",
					Fields = new List<FieldRef>
					{
						new FieldRef
						{
							Name = "Field Type"
						}
					},
					IncludeNameInQueryResult = true
				};
				QueryResult result = await objectManager.QueryAsync(workspaceId, request, 0, 1).ConfigureAwait(false);
				return result.Objects.First().Name;
			}
		}

		public static Task<IList<int>> QueryDocumentIdsAsync(ServiceFactory serviceFactory, int workspaceId)
		{
			return QueryDocumentIdsAsync(serviceFactory, workspaceId, string.Empty);
		}

		public static async Task<IList<int>> QueryDocumentIdsAsync(ServiceFactory serviceFactory, int workspaceId,
			string condition)
		{
			IList<RelativityObject> documents =
				await QueryDocumentsAsync(serviceFactory, workspaceId, condition).ConfigureAwait(false);
			IList<int> identifiers = documents.Select(x => x.ArtifactID).ToList();
			return identifiers;
		}

		public static Task<IList<string>> GetAllDocumentIdentifiersAsync(ServiceFactory serviceFactory, int workspaceId)
		{
			return QueryDocumentIdentifiersAsync(serviceFactory, workspaceId, string.Empty);
		}

		public static async Task<IList<string>> QueryDocumentIdentifiersAsync(ServiceFactory serviceFactory,
			int workspaceId, string condition)
		{
			IList<RelativityObject> documents =
				await QueryDocumentsAsync(serviceFactory, workspaceId, condition).ConfigureAwait(false);
			IList<string> identifiers = documents.Select(x => x.Name).ToList();
			return identifiers;
		}

		public static async Task<int> CreateSyncConfigurationRDOAsync(ServiceFactory serviceFactory, int workspaceId,
			FullSyncJobConfiguration configuration, ISerializer serializer = null)
		{
			CreateRequest request =
				PrepareSyncConfigurationCreateRequestAsync(configuration, serializer ?? new JSONSerializer());
			using (IObjectManager objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				CreateResult createResult = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);

				return createResult.Object.ArtifactID;
			}
		}

		private static async Task<IList<RelativityObject>> QueryDocumentsAsync(ServiceFactory serviceFactory,
			int workspaceId, string condition)
		{
			using (IObjectManager objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				var documents = new List<RelativityObject>();
				var queryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID },
					Condition = condition,
					IncludeNameInQueryResult = true
				};

				QueryResult queryResult;
				do
				{
					const int batchSize = 100;
					queryResult = await objectManager.QueryAsync(workspaceId, queryRequest, documents.Count, batchSize)
						.ConfigureAwait(false);
					documents.AddRange(queryResult.Objects);
				}
				while (documents.Count < queryResult.TotalCount);

				return documents;
			}
		}


		public static async Task<RelativityObject> GetJobHistoryAsync(ServiceFactory serviceFactory, int workspaceId, int jobHistoryId)
		{
			using (IObjectManager objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = new Guid("08F4B1F7-9692-4A08-94AB-B5F3A88B6CC9")
					},
					Condition = $"'ArtifactId' == {jobHistoryId}",
					Fields = new FieldRef[]
					{
						new FieldRef
						{
							Name = "*"
						}
					}
				};

				QueryResult result = await objectManager.QueryAsync(workspaceId, request, 1, 1).ConfigureAwait(false);

				return result.Objects.FirstOrDefault();
			}
		}

		private static CreateRequest PrepareSyncConfigurationCreateRequestAsync(FullSyncJobConfiguration configuration,
			ISerializer serializer)
		{
			return new CreateRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57")
				},
				ParentObject = new RelativityObjectRef
				{
					ArtifactID = configuration.JobHistoryId
				},
				FieldValues = new[]
				{
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = CreateSavedSearchInDestinationGuid
						},
						Value = configuration.CreateSavedSearchForTagging
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = DataDestinationArtifactIdGuid
						},
						Value = configuration.DestinationFolderArtifactId
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
						Value = configuration.SavedSearchArtifactId
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
						Value = configuration.DestinationFolderStructureBehavior.ToString()
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = FolderPathSourceFieldNameGuid
						},
						Value = configuration.FolderPathSourceFieldName
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = DestinationWorkspaceArtifactIdGuid
						},
						Value = configuration.TargetWorkspaceArtifactId
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = EmailNotificationRecipientsGuid
						},
						Value = configuration.EmailNotificationRecipients
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = FieldMappingsGuid
						},
						Value = serializer.Serialize(configuration.FieldsMapping)
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = FieldOverlayBehaviorGuid
						},
						Value = configuration.FieldOverlayBehavior.ToString()
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = ImportOverwriteModeGuid
						},
						Value = configuration.ImportOverwriteMode.ToString()
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = MoveExistingDocumentsGuid
						},
						Value = configuration.MoveExistingDocuments
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = NativesBehaviorGuid
						},
						Value = configuration.ImportNativeFileCopyMode.ToString()
					},
					new FieldRefValuePair
					{
						Field = new FieldRef
						{
							Guid = RdoArtifactTypeIdGuid
						},
						Value = (int) ArtifactType.Document
					}
				}
			};
		}
	}
}