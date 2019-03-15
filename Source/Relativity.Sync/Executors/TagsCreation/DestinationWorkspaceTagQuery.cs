using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SourceWorkspaceTagsCreation;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.TagsCreation
{
	internal sealed class DestinationWorkspaceTagQuery : IDestinationWorkspaceTagQuery
	{
		private const int _FEDERATED_INSTANCE_ID = -1;

		private const string _DESTINATION_WORKSPACE_ARTIFACTID_GUID = @"207e6836-2961-466b-a0d2-29974a4fad36";
		private const string _DESTINATION_WORKSPACE_NAME_GUID = @"348d7394-2658-4da4-87d0-8183824adf98";
		private const string _DESTINATION_INSTANCE_NAME_GUID = @"909adc7c-2bb9-46ca-9f85-da32901d6554";
		private const string _DESTINATION_INSTANCE_ARTIFACTID_GUID = @"323458db-8a06-464b-9402-af2516cf47e0";
		private const string _NAME_GUID = @"155649c0-db15-4ee7-b449-bfdf2a54b7b5";

		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;

		public DestinationWorkspaceTagQuery(ISourceServiceFactoryForUser sourceServiceFactoryForUser)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
		}

		public async Task<DestinationWorkspaceTag> QueryAsync(ISourceWorkspaceTagsCreationConfiguration configuration)
		{
			RelativityObject tag = await QueryRelativityObjectTag(configuration).ConfigureAwait(false);

			if (tag != null)
			{
				DestinationWorkspaceTag destinationWorkspaceTag = new DestinationWorkspaceTag();
				destinationWorkspaceTag.ArtifactId = tag.ArtifactID;
				destinationWorkspaceTag.DestinationWorkspaceName = tag[_DESTINATION_WORKSPACE_NAME_GUID].Value.ToString();
				destinationWorkspaceTag.DestinationInstanceName = tag[_DESTINATION_INSTANCE_NAME_GUID].Value.ToString();
				return destinationWorkspaceTag;
			}

			return null;
		}

		private async Task<RelativityObject> QueryRelativityObjectTag(ISourceWorkspaceTagsCreationConfiguration configuration)
		{
			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest queryRequest = new QueryRequest()
				{
					Condition = $"'{_DESTINATION_WORKSPACE_ARTIFACTID_GUID}' == {configuration.DestinationWorkspaceArtifactId} AND '{_DESTINATION_INSTANCE_ARTIFACTID_GUID}' == {_FEDERATED_INSTANCE_ID}",
					Fields = new List<FieldRef>
					{
						new FieldRef { Name = "ArtifactId" },
						new FieldRef { Guid = new Guid(_DESTINATION_WORKSPACE_NAME_GUID)},
						new FieldRef { Guid = new Guid(_DESTINATION_INSTANCE_NAME_GUID) },
						new FieldRef { Guid = new Guid(_DESTINATION_WORKSPACE_ARTIFACTID_GUID) },
						new FieldRef { Guid = new Guid(_DESTINATION_INSTANCE_ARTIFACTID_GUID) },
						new FieldRef { Guid = new Guid(_NAME_GUID) }
					}
				};
				QueryResult queryResult = await objectManager.QueryAsync(configuration.SourceWorkspaceArtifactId, queryRequest, 0, 1).ConfigureAwait(false);
				return queryResult.Objects.FirstOrDefault();
			}
		}
	}
}