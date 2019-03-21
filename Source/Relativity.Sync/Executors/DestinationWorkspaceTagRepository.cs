using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceTagRepository : IDestinationWorkspaceTagRepository
	{
		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly IFederatedInstance _federatedInstance;
		private readonly ITagNameFormatter _tagNameFormatter;
		private readonly ISyncLog _logger;

		private static readonly Guid ObjectTypeGuid = new Guid("3F45E490-B4CF-4C7D-8BB6-9CA891C0C198");
		private static readonly Guid DestinationWorkspaceNameGuid = new Guid("348d7394-2658-4da4-87d0-8183824adf98");
		private static readonly Guid DestinationInstanceNameGuid = new Guid("909adc7c-2bb9-46ca-9f85-da32901d6554");
		private static readonly Guid DestinationInstanceArtifactidGuid = new Guid("323458db-8a06-464b-9402-af2516cf47e0");
		private static readonly Guid NameGuid = new Guid("155649c0-db15-4ee7-b449-bfdf2a54b7b5");
		private static readonly Guid DestinationWorkspaceArtifactidGuid = new Guid("207e6836-2961-466b-a0d2-29974a4fad36");

		public DestinationWorkspaceTagRepository(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IFederatedInstance federatedInstance, ITagNameFormatter tagNameFormatter,
			ISyncLog logger)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_federatedInstance = federatedInstance;
			_logger = logger;
			_tagNameFormatter = tagNameFormatter;
		}
		
		public async Task<DestinationWorkspaceTag> ReadAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, CancellationToken token)
		{
			_logger.LogVerbose($"Reading {nameof(DestinationWorkspaceTag)}. Source workspace artifact ID: {{sourceWorkspaceArtifactId}} " +
				"Destination workspace artifact ID: {destinationWorkspaceArtifactId}",
				sourceWorkspaceArtifactId, destinationWorkspaceArtifactId);
			RelativityObject tag = await QueryRelativityObjectTagAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, token).ConfigureAwait(false);

			if (tag != null)
			{
				DestinationWorkspaceTag destinationWorkspaceTag = new DestinationWorkspaceTag
				{
					ArtifactId = tag.ArtifactID,
					DestinationWorkspaceName = tag[DestinationWorkspaceNameGuid].Value.ToString(),
					DestinationInstanceName = tag[DestinationInstanceNameGuid].Value.ToString(),
					DestinationWorkspaceArtifactId = Convert.ToInt32(tag[DestinationWorkspaceArtifactidGuid].Value, CultureInfo.InvariantCulture)
				};
				return destinationWorkspaceTag;
			}

			return null;
		}

		private async Task<RelativityObject> QueryRelativityObjectTagAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, CancellationToken token)
		{
			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				int federatedInstanceId = await _federatedInstance.GetInstanceIdAsync().ConfigureAwait(false);

#pragma warning disable S1135 // Track uses of "TODO" tags
				// TODO REL-304544: We use -1 as the instance ID for the local instance, but RIP uses NULL, so we have to use NULL instead in that case.
				// TODO REL-304544: Once the Sync code is entirely in this project, this should be changed to just use -1.
#pragma warning restore S1135 // Track uses of "TODO" tags
				string federatedInstanceIdSearchTerm = federatedInstanceId == -1
					? $"NOT '{DestinationInstanceArtifactidGuid}' ISSET"
					: $"'{DestinationInstanceArtifactidGuid}' == {federatedInstanceId}";

				QueryRequest request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuid },
					Condition = $"'{DestinationWorkspaceArtifactidGuid}' == {destinationWorkspaceArtifactId} AND ({federatedInstanceIdSearchTerm})",
					Fields = new List<FieldRef>
					{
						new FieldRef { Name = "ArtifactId" },
						new FieldRef { Guid = DestinationWorkspaceNameGuid},
						new FieldRef { Guid = DestinationInstanceNameGuid },
						new FieldRef { Guid = DestinationWorkspaceArtifactidGuid },
						new FieldRef { Guid = DestinationInstanceArtifactidGuid },
						new FieldRef { Guid = NameGuid }
					}
				};
				QueryResult queryResult;
				try
				{
					const int start = 0;
					const int length = 1;
					queryResult = await objectManager.QueryAsync(sourceWorkspaceArtifactId, request, start, length, token, new EmptyProgress<ProgressReport>()).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex, $"Service call failed while querying {nameof(DestinationWorkspaceTag)} object: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Service call failed while querying {nameof(DestinationWorkspaceTag)} in workspace {sourceWorkspaceArtifactId}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to query {nameof(DestinationWorkspaceTag)} object: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Failed to query {nameof(DestinationWorkspaceTag)} in workspace {sourceWorkspaceArtifactId}", ex);
				}

				return queryResult.Objects.FirstOrDefault();
			}
		}

		public async Task<DestinationWorkspaceTag> CreateAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName)
		{
			_logger.LogVerbose($"Creating {nameof(DestinationWorkspaceTag)} in source workspace ID: {{sourceWorkspaceArtifactId}} " +
					"Destination workspace ID: {destinationWorkspaceArtifactId} " +
					"Destination workspace name: {destinationWorkspaceName}",
					sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, destinationWorkspaceName);
			string federatedInstanceName = await _federatedInstance.GetInstanceNameAsync().ConfigureAwait(false);

#pragma warning disable S1135 // Track uses of "TODO" tags
			// TODO REL-304544: We use -1 as the instance ID for the local instance, but RIP uses NULL, so we have to use NULL instead in that case.
			// TODO REL-304544: Once the Sync code is entirely in this project, this should be changed to just use -1.
#pragma warning restore S1135 // Track uses of "TODO" tags
			int rawFederatedInstanceId = await _federatedInstance.GetInstanceIdAsync().ConfigureAwait(false);
			int? federatedInstanceId = rawFederatedInstanceId == -1 ? null : (int?) rawFederatedInstanceId;

			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				CreateRequest request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuid },
					FieldValues = CreateFieldValues(destinationWorkspaceArtifactId, destinationWorkspaceName, federatedInstanceName, federatedInstanceId)
				};

				CreateResult result;
				try
				{
					result = await objectManager.CreateAsync(sourceWorkspaceArtifactId, request).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex, $"Service call failed while creating {nameof(DestinationWorkspaceTag)}: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Service call failed while creating {nameof(DestinationWorkspaceTag)}: {request}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to create {nameof(DestinationWorkspaceTag)}: {{request}}", request);
					string tagName = request.FieldValues.First(x => x.Field.Guid == NameGuid).Value.ToString();
					throw new DestinationWorkspaceTagRepositoryException(
						$"Failed to create {nameof(DestinationWorkspaceTag)} '{tagName}' in workspace {sourceWorkspaceArtifactId}",
						ex);
				}

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
			_logger.LogVerbose($"Updating {nameof(DestinationWorkspaceTag)} in source workspace ID: {{sourceWorkspaceArtifactId}}", sourceWorkspaceArtifactId);
			string federatedInstanceName = await _federatedInstance.GetInstanceNameAsync().ConfigureAwait(false);
			int federatedInstanceId = await _federatedInstance.GetInstanceIdAsync().ConfigureAwait(false);

			UpdateRequest request = new UpdateRequest
			{
				Object = new RelativityObjectRef { ArtifactID =  destinationWorkspaceTag.ArtifactId },
				FieldValues = CreateFieldValues(destinationWorkspaceTag.DestinationWorkspaceArtifactId, destinationWorkspaceTag.DestinationWorkspaceName, federatedInstanceName, federatedInstanceId),
			};

			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				try
				{
					await objectManager.UpdateAsync(sourceWorkspaceArtifactId, request).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex, $"Service call failed while updating {nameof(DestinationWorkspaceTag)}: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Failed to update {nameof(DestinationWorkspaceTag)} with id {destinationWorkspaceTag.ArtifactId} in workspace {sourceWorkspaceArtifactId}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to update {nameof(DestinationWorkspaceTag)}: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Failed to update {nameof(DestinationWorkspaceTag)} with id {destinationWorkspaceTag.ArtifactId} in workspace {sourceWorkspaceArtifactId}", ex);
				}
			}
		}

		private IEnumerable<FieldRefValuePair> CreateFieldValues(int destinationWorkspaceArtifactId, string destinationWorkspaceName, string federatedInstanceName, int? federatedInstanceId)
		{
			string destinationTagName = _tagNameFormatter.FormatWorkspaceDestinationTagName(federatedInstanceName, destinationWorkspaceName, destinationWorkspaceArtifactId);
			FieldRefValuePair[] pairs =
			{
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = NameGuid},
					Value = destinationTagName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = DestinationWorkspaceNameGuid},
					Value = destinationWorkspaceName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = DestinationWorkspaceArtifactidGuid},
					Value = destinationWorkspaceArtifactId
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = DestinationInstanceNameGuid},
					Value = federatedInstanceName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = DestinationInstanceArtifactidGuid},
					Value = federatedInstanceId
				}
			};

			return pairs;
		}
	}
}