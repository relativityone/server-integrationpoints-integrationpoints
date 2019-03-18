using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceTagRepository : IDestinationWorkspaceTagRepository
	{
		private const int _NAME_MAX_LENGTH = 255;

		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly IFederatedInstance _federatedInstance;
		private readonly ISyncLog _logger;

		private static readonly Guid _OBJECT_TYPE_GUID = new Guid("3F45E490-B4CF-4C7D-8BB6-9CA891C0C198");
		private static readonly Guid _DESTINATION_WORKSPACE_NAME_GUID = new Guid("348d7394-2658-4da4-87d0-8183824adf98");
		private static readonly Guid _DESTINATION_INSTANCE_NAME_GUID = new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554");
		private static readonly Guid _DESTINATION_INSTANCE_ARTIFACTID_GUID = new Guid("323458db-8a06-464b-9402-af2516cf47e0");
		private static readonly Guid _NAME_GUID = new Guid("155649c0-db15-4ee7-b449-bfdf2a54b7b5");
		private static readonly Guid _DESTINATION_WORKSPACE_ARTIFACTID_GUID = new Guid("207e6836-2961-466b-a0d2-29974a4fad36");

		public DestinationWorkspaceTagRepository(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IFederatedInstance federatedInstance, ISyncLog logger)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_federatedInstance = federatedInstance;
			_logger = logger;
		}
		
		public async Task<DestinationWorkspaceTag> ReadAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
		{
			_logger.LogVerbose("Reading destination workspace tag. Source workspace artifact ID: {sourceWorkspaceArtifactId} " +
				"Destination workspace artifact ID: {destinationWorkspaceArtifactId}",
				sourceWorkspaceArtifactId, destinationWorkspaceArtifactId);
			RelativityObject tag = await QueryRelativityObjectTagAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId).ConfigureAwait(false);

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

		private async Task<RelativityObject> QueryRelativityObjectTagAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId)
		{
			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				int federatedInstanceId = await _federatedInstance.GetInstanceIdAsync().ConfigureAwait(false);
				QueryRequest queryRequest = new QueryRequest()
				{
					Condition = $"'{_DESTINATION_WORKSPACE_ARTIFACTID_GUID}' == {destinationWorkspaceArtifactId} AND '{_DESTINATION_INSTANCE_ARTIFACTID_GUID}' == {federatedInstanceId}",
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
				const int start = 0;
				const int length = 1;
				QueryResult queryResult = await objectManager.QueryAsync(sourceWorkspaceArtifactId, queryRequest, start, length).ConfigureAwait(false);
				return queryResult.Objects.FirstOrDefault();
			}
		}

		public async Task<DestinationWorkspaceTag> CreateAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName)
		{
			_logger.LogVerbose("Creating destination workspace tag in source workspace ID: {sourceWorkspaceArtifactId} " +
					"Destination workspace ID: {destinationWorkspaceArtifactId} " +
					"Destination workspace name: {destinationWorkspaceName}",
					sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, destinationWorkspaceName);
			string federatedInstanceName = await _federatedInstance.GetInstanceNameAsync().ConfigureAwait(false);
			int federatedInstanceId = await _federatedInstance.GetInstanceIdAsync().ConfigureAwait(false);

			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				CreateRequest request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef { Guid = _OBJECT_TYPE_GUID },
					FieldValues = CreateFieldValues(destinationWorkspaceArtifactId, destinationWorkspaceName, federatedInstanceName, federatedInstanceId)
				};

				CreateResult result = await objectManager.CreateAsync(sourceWorkspaceArtifactId, request).ConfigureAwait(false);

				DestinationWorkspaceTag createdTag = new DestinationWorkspaceTag
				{
					ArtifactId = result.Object.ArtifactID,
					DestinationWorkspaceName = destinationWorkspaceName,
					DestinationInstanceName = federatedInstanceName,
					DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId
				};
				return createdTag;
			}
		}

		public async Task UpdateAsync(int sourceWorkspaceArtifactId, DestinationWorkspaceTag destinationWorkspaceTag)
		{
			_logger.LogVerbose("Updating destination workspace tag in source workspace ID: {sourceWorkspaceArtifactId}", sourceWorkspaceArtifactId);
			string federatedInstanceName = await _federatedInstance.GetInstanceNameAsync().ConfigureAwait(false);
			int federatedInstanceId = await _federatedInstance.GetInstanceIdAsync().ConfigureAwait(false);

			UpdateRequest request = new UpdateRequest
			{
				Object = new RelativityObjectRef { ArtifactID =  destinationWorkspaceTag.ArtifactId },
				FieldValues = CreateFieldValues(destinationWorkspaceTag.DestinationWorkspaceArtifactId, destinationWorkspaceTag.DestinationWorkspaceName, federatedInstanceName, federatedInstanceId),
			};

			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				await objectManager.UpdateAsync(sourceWorkspaceArtifactId, request).ConfigureAwait(false);
			}
		}

		private IEnumerable<FieldRefValuePair> CreateFieldValues(int destinationWorkspaceArtifactId, string destinationWorkspaceName, string federatedInstanceName, int federatedInstanceId)
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
					Value = federatedInstanceId
				}
			};

			return pairs;
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

		private static string GetFormatForWorkspaceOrJobDisplay(string prefix, string name, int? id)
		{
			return $"{prefix} - {GetFormatForWorkspaceOrJobDisplay(name, id)}";
		}

		private static string GetFormatForWorkspaceOrJobDisplay(string name, int? id)
		{
			return id.HasValue ? $"{name} - {id}" : name;
		}
	}
}