using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal static class Rdos
	{
		private const int _DOCUMENT_ARTIFACT_TYPE_ID = 10;

		private static readonly Guid _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_GUID = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
		private static readonly Guid _RELATIVITY_SOURCE_JOB_OBJECT_TYPE_GUID = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");
		private static readonly Guid _DESTINATION_WORKSPACE_OBJECT_TYPE_GUID = new Guid("3F45E490-B4CF-4C7D-8BB6-9CA891C0C198");

		private static readonly Guid _RELATIVITY_SOURCE_CASE_ID_FIELD_GUID = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
		private static readonly Guid _RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");
		private static readonly Guid _RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
		private static readonly Guid _RELATIVITY_SOURCE_JOB_JOB_HISTORY_ID_FIELD_GUID = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
		private static readonly Guid _RELATIVITY_SOURCE_JOB_JOB_HISTORY_NAME_FIELD_GUID = new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");
		private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_ARTIFACTID_GUID = new Guid("323458db-8a06-464b-9402-af2516cf47e0");
		private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_NAME_GUID = new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554");
		private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_ARTIFACTID_GUID = new Guid("207e6836-2961-466b-a0d2-29974a4fad36");
		private static readonly Guid _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_NAME_GUID = new Guid("348d7394-2658-4da4-87d0-8183824adf98");
		private static readonly Guid _DESTINATION_WORKSPACE_NAME_GUID = new Guid("155649c0-db15-4ee7-b449-bfdf2a54b7b5");

		public static async Task<int> CreateJobHistoryInstance(ServiceFactory serviceFactory, int workspaceId, string name = "Name")
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

		public static async Task<int> CreateSyncConfigurationInstance(ServiceFactory serviceFactory, int workspaceId, int jobHistoryId)
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
					}
				};
				CreateResult result = await objectManager.CreateAsync(workspaceId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		public static async Task<int> CreateRelativitySourceCaseInstance(ServiceFactory serviceFactory, int destinationWorkspaceArtifactId, RelativitySourceCaseTag tag)
		{
			using (var objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				CreateRequest request = new CreateRequest
				{
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Field = new FieldRef { Name = "Name"},
							Value = tag.Name
						},new FieldRefValuePair
						{
							Field = new FieldRef { Guid = _RELATIVITY_SOURCE_CASE_ID_FIELD_GUID},
							Value = tag.SourceWorkspaceArtifactId
						},
						new FieldRefValuePair
						{
							Field = new FieldRef { Guid = _RELATIVITY_SOURCE_CASE_NAME_FIELD_GUID },
							Value = tag.SourceWorkspaceName
						},
						new FieldRefValuePair
						{
							Field = new FieldRef { Guid = _RELATIVITY_SOURCE_CASE_INSTANCE_NAME_FIELD_GUID },
							Value = tag.SourceInstanceName
						}
					},
					ObjectType = new ObjectTypeRef
					{
						Guid = _RELATIVITY_SOURCE_CASE_OBJECT_TYPE_GUID
					}
				};
				CreateResult result = await objectManager.CreateAsync(destinationWorkspaceArtifactId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		public static async Task<int> CreateRelativitySourceJobInstance(ServiceFactory serviceFactory, int destinationWorkspaceArtifactId, RelativitySourceJobTag tag)
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
				CreateResult result = await objectManager.CreateAsync(destinationWorkspaceArtifactId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		public static async Task<int> CreateDestinationWorkspaceTagInstance(ServiceFactory serviceFactory, int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName)
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
							Field = new FieldRef { Guid = _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_ARTIFACTID_GUID },
							Value = destinationWorkspaceArtifactId
						},
						new FieldRefValuePair
						{
							Field = new FieldRef { Guid = _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_ARTIFACTID_GUID },
							Value = -1
						},
						new FieldRefValuePair
						{
							Field = new FieldRef { Guid = _DESTINATION_WORKSPACE_DESTINATION_INSTANCE_NAME_GUID },
							Value = destinationInstanceName
						},
						new FieldRefValuePair
						{
							Field = new FieldRef { Guid = _DESTINATION_WORKSPACE_DESTINATION_WORKSPACE_NAME_GUID },
							Value = destinationWorkspaceName
						},
						new FieldRefValuePair
						{
							Field = new FieldRef { Guid = _DESTINATION_WORKSPACE_NAME_GUID },
							Value = tagName
						}
					}
				};

				CreateResult result = await objectManager.CreateAsync(sourceWorkspaceArtifactId, request).ConfigureAwait(false);
				return result.Object.ArtifactID;
			}
		}

		public static async Task<int> GetSavedSearchInstance(ServiceFactory serviceFactory, int workspaceId, string name = "All Documents")
		{
			using (IKeywordSearchManager keywordSearchManager = serviceFactory.CreateProxy<IKeywordSearchManager>())
			{
				Services.Query request = new Services.Query()
				{
					Condition = $"(('Name' == '{name}'))"
				};
				KeywordSearchQueryResultSet result = await keywordSearchManager.QueryAsync(workspaceId, request).ConfigureAwait(false);
				if (result.TotalCount == 0)
				{
					throw new InvalidOperationException($"Cannot find saved search '{name}' in workspace {workspaceId}");
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

		public static async Task<int> GetFolderPathSourceField(ServiceFactory serviceFactory, int workspaceId)
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
					Fields = new List<FieldRef>()
					{
						new FieldRef()
						{
							Name = "Field Type"
						}
					}
				};
				QueryResult result = await objectManager.QueryAsync(workspaceId, request, 0, 1).ConfigureAwait(false);
				return result.Objects.First().ArtifactID;
			}
		}

		public static Task<IList<int>> GetAllDocumentsAsync(ServiceFactory serviceFactory, int workspaceId)
		{
			return QueryDocumentsAsync(serviceFactory, workspaceId, string.Empty);
		}

		public static async Task<IList<int>> QueryDocumentsAsync(ServiceFactory serviceFactory, int workspaceId, string condition)
		{
			using (IObjectManager objectManager = serviceFactory.CreateProxy<IObjectManager>())
			{
				var ids = new List<int>();

				QueryRequest queryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID },
					Condition = condition
				};

				QueryResult queryResult;
				do
				{
					const int batchSize = 100;
					queryResult = await objectManager.QueryAsync(workspaceId, queryRequest, ids.Count, batchSize).ConfigureAwait(false);
					ids.AddRange(queryResult.Objects.Select(x => x.ArtifactID));
				}
				while (ids.Count < queryResult.TotalCount);

				return ids;
			}
		}
	}
}