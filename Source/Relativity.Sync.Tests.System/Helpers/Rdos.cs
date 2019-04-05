using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal static class Rdos
	{
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
	}
}