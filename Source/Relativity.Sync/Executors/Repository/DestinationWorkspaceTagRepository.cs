using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Executors.SourceWorkspaceTagsCreation;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors.Repository
{
	internal sealed class DestinationWorkspaceTagRepository : IDestinationWorkspaceTagRepository
	{
		private const int _FEDERATED_INSTANCE_ID = -1;
		private const string _DEFAULT_DESTINATION_WORKSPACE_INSTANCE_NAME = "This Instance";
		private const int _NAME_MAX_LENGTH = 255;

		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly ISyncLog _logger;

		private static readonly Guid _OBJECT_TYPE_GUID = new Guid("3F45E490-B4CF-4C7D-8BB6-9CA891C0C198");
		private static readonly Guid _DESTINATION_WORKSPACE_NAME_GUID = new Guid("348d7394-2658-4da4-87d0-8183824adf98");
		private static readonly Guid _DESTINATION_INSTANCE_NAME_GUID = new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554");
		private static readonly Guid _DESTINATION_INSTANCE_ARTIFACTID_GUID = new Guid("323458db-8a06-464b-9402-af2516cf47e0");
		private static readonly Guid _NAME_GUID = new Guid("155649c0-db15-4ee7-b449-bfdf2a54b7b5");
		private static readonly Guid _DESTINATION_WORKSPACE_ARTIFACTID_GUID = new Guid("207e6836-2961-466b-a0d2-29974a4fad36");

		public DestinationWorkspaceTagRepository(ISourceServiceFactoryForUser sourceServiceFactoryForUser, ISyncLog logger)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_logger = logger;
		}


		public async Task<DestinationWorkspaceTag> QueryAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
		{
			RelativityObject tag = await QueryRelativityObjectTag(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId).ConfigureAwait(false);

			if (tag != null)
			{
				DestinationWorkspaceTag destinationWorkspaceTag = new DestinationWorkspaceTag
				{
					ArtifactId = tag.ArtifactID,
					DestinationWorkspaceName = tag[_DESTINATION_WORKSPACE_NAME_GUID].Value.ToString(),
					DestinationInstanceName = tag[_DESTINATION_INSTANCE_NAME_GUID].Value.ToString(),
					DestinationWorkspaceArtifactId = Convert.ToInt32(tag[_DESTINATION_WORKSPACE_ARTIFACTID_GUID].Value, CultureInfo.InvariantCulture)
				};
				return destinationWorkspaceTag;
			}

			return null;
		}

		private async Task<RelativityObject> QueryRelativityObjectTag(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
		{
			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest queryRequest = new QueryRequest()
				{
					Condition = $"'{_DESTINATION_WORKSPACE_ARTIFACTID_GUID}' == {destinationWorkspaceArtifactId} AND '{_DESTINATION_INSTANCE_ARTIFACTID_GUID}' == {_FEDERATED_INSTANCE_ID}",
					Fields = new List<FieldRef>
					{
						new FieldRef { Name = "ArtifactId" },
						new FieldRef { Guid = _DESTINATION_WORKSPACE_NAME_GUID},
						new FieldRef { Guid = _DESTINATION_INSTANCE_NAME_GUID },
						new FieldRef { Guid = _DESTINATION_WORKSPACE_ARTIFACTID_GUID },
						new FieldRef { Guid = _DESTINATION_INSTANCE_ARTIFACTID_GUID },
						new FieldRef { Guid = _NAME_GUID }
					}
				};
				QueryResult queryResult = await objectManager.QueryAsync(sourceWorkspaceArtifactId, queryRequest, 0, 1).ConfigureAwait(false);
				return queryResult.Objects.FirstOrDefault();
			}
		}

		public async Task<DestinationWorkspaceTag> CreateAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName)
		{
			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				CreateRequest request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef { Guid = _OBJECT_TYPE_GUID },
					FieldValues = CreateFieldValues(destinationWorkspaceArtifactId, destinationWorkspaceName, _DEFAULT_DESTINATION_WORKSPACE_INSTANCE_NAME)
				};

				CreateResult result = await objectManager.CreateAsync(sourceWorkspaceArtifactId, request).ConfigureAwait(false);

				DestinationWorkspaceTag createdTag = new DestinationWorkspaceTag
				{
					ArtifactId = result.Object.ArtifactID,
					DestinationWorkspaceName = destinationWorkspaceName,
					DestinationInstanceName = _DEFAULT_DESTINATION_WORKSPACE_INSTANCE_NAME,
					DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId
				};
				return createdTag;
			}
		}

		public async Task UpdateAsync(int sourceWorkspaceArtifactId, DestinationWorkspaceTag destinationWorkspaceTag)
		{
			UpdateRequest request = new UpdateRequest
			{
				Object = new RelativityObjectRef { ArtifactID =  destinationWorkspaceTag.ArtifactId },
				FieldValues = CreateFieldValues(destinationWorkspaceTag.DestinationWorkspaceArtifactId, destinationWorkspaceTag.DestinationWorkspaceName, _DEFAULT_DESTINATION_WORKSPACE_INSTANCE_NAME),
			};

			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				await objectManager.UpdateAsync(sourceWorkspaceArtifactId, request).ConfigureAwait(false);
			}
		}

		private IEnumerable<FieldRefValuePair> CreateFieldValues(int destinationWorkspaceArtifactId, string destinationWorkspaceName, string federatedInstanceName)
		{
			FieldRefValuePair[] pairs =
			{
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = _NAME_GUID},
					Value = FormatWorkspaceDestinationTagName(federatedInstanceName, destinationWorkspaceName, destinationWorkspaceArtifactId)
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = _DESTINATION_WORKSPACE_NAME_GUID},
					Value = destinationWorkspaceName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = _DESTINATION_WORKSPACE_ARTIFACTID_GUID},
					Value = destinationWorkspaceArtifactId
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = _DESTINATION_INSTANCE_NAME_GUID},
					Value = federatedInstanceName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = _DESTINATION_INSTANCE_ARTIFACTID_GUID},
					Value = _FEDERATED_INSTANCE_ID
				}
			};

			return pairs;
		}

		private static string GetFormatForWorkspaceOrJobDisplay(string name, int? id)
		{
			return id.HasValue ? $"{name} - {id}" : name;
		}

		private static string GetFormatForWorkspaceOrJobDisplay(string prefix, string name, int? id)
		{
			return $"{prefix} - {GetFormatForWorkspaceOrJobDisplay(name, id)}";
		}

		private string FormatWorkspaceDestinationTagName(string federatedInstanceName, string destinationWorkspaceName, int destinationWorkspaceArtifactId)
		{
			string name = GetFormatForWorkspaceOrJobDisplay(federatedInstanceName, destinationWorkspaceName, destinationWorkspaceArtifactId);
			if (name.Length > _NAME_MAX_LENGTH)
			{
				_logger.LogWarning("Relativity Source Case Name exceeded max length and has been shortened. Full name {name}.", name);

				int overflow = name.Length - _NAME_MAX_LENGTH;
				string trimmedInstanceName = federatedInstanceName.Substring(0, federatedInstanceName.Length - overflow);
				name = GetFormatForWorkspaceOrJobDisplay(trimmedInstanceName, destinationWorkspaceName, destinationWorkspaceArtifactId);
			}

			return name;
		}
	}
}