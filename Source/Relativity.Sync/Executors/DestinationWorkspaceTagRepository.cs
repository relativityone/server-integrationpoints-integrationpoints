﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Executors
{
	internal sealed class DestinationWorkspaceTagRepository : IDestinationWorkspaceTagRepository
	{
		private const int _MAX_OBJECT_QUERY_BATCH_SIZE = 10000;

		private readonly IFederatedInstance _federatedInstance;
		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly ISyncLog _logger;
		private readonly ITagNameFormatter _tagNameFormatter;
		private readonly ISyncMetrics _syncMetrics;

		private readonly Guid _destinationInstanceArtifactIdGuid = new Guid("323458DB-8A06-464B-9402-AF2516CF47E0");
		private readonly Guid _destinationInstanceNameGuid = new Guid("909ADC7C-2BB9-46CA-9F85-DA32901D6554");
		private readonly Guid _destinationWorkspaceArtifactIdGuid = new Guid("207E6836-2961-466B-A0D2-29974A4FAD36");
		private readonly Guid _destinationWorkspaceFieldMultiObject = new Guid("8980C2FA-0D33-4686-9A97-EA9D6F0B4196");
		private readonly Guid _destinationWorkspaceNameGuid = new Guid("348D7394-2658-4DA4-87D0-8183824ADF98");
		private readonly Guid _jobHistoryFieldMultiObject = new Guid("97BC12FA-509B-4C75-8413-6889387D8EF6");
		private readonly Guid _nameGuid = new Guid("155649C0-DB15-4EE7-B449-BFDF2A54B7B5");
		private readonly Guid _objectTypeGuid = new Guid("3F45E490-B4CF-4C7D-8BB6-9CA891C0C198");

		public DestinationWorkspaceTagRepository(ISourceServiceFactoryForUser sourceServiceFactoryForUser, IFederatedInstance federatedInstance, ITagNameFormatter tagNameFormatter,
			ISyncLog logger, ISyncMetrics syncMetrics)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_federatedInstance = federatedInstance;
			_logger = logger;
			_syncMetrics = syncMetrics;
			_tagNameFormatter = tagNameFormatter;
		}

		public async Task<DestinationWorkspaceTag> ReadAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, CancellationToken token)
		{
			_logger.LogVerbose($"Reading {nameof(DestinationWorkspaceTag)}. Source workspace artifact ID: {{sourceWorkspaceArtifactId}} " +
				"Destination workspace artifact ID: {destinationWorkspaceArtifactId}",
				sourceWorkspaceArtifactId, destinationWorkspaceArtifactId);
			RelativityObject tag = await QueryRelativityObjectTagAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, token).ConfigureAwait(false);

			DestinationWorkspaceTag destinationWorkspaceTag = null;
			if (tag != null)
			{
				destinationWorkspaceTag = new DestinationWorkspaceTag
				{
					ArtifactId = tag.ArtifactID,
					DestinationWorkspaceName = tag[_destinationWorkspaceNameGuid].Value.ToString(),
					DestinationInstanceName = tag[_destinationInstanceNameGuid].Value.ToString(),
					DestinationWorkspaceArtifactId = Convert.ToInt32(tag[_destinationWorkspaceArtifactIdGuid].Value, CultureInfo.InvariantCulture)
				};
			}
			return destinationWorkspaceTag;
		}

		private async Task<RelativityObject> QueryRelativityObjectTagAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, CancellationToken token)
		{
			using (var objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				int federatedInstanceId = await _federatedInstance.GetInstanceIdAsync().ConfigureAwait(false);

				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { Guid = _objectTypeGuid },
					Condition = $"'{_destinationWorkspaceArtifactIdGuid}' == {destinationWorkspaceArtifactId} AND ('{_destinationInstanceArtifactIdGuid}' == {federatedInstanceId})",
					Fields = new List<FieldRef>
					{
						new FieldRef { Name = "ArtifactId" },
						new FieldRef { Guid = _destinationWorkspaceNameGuid},
						new FieldRef { Guid = _destinationInstanceNameGuid },
						new FieldRef { Guid = _destinationWorkspaceArtifactIdGuid },
						new FieldRef { Guid = _destinationInstanceArtifactIdGuid },
						new FieldRef { Guid = _nameGuid }
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
					throw new SyncKeplerException($"Service call failed while querying {nameof(DestinationWorkspaceTag)} in workspace {sourceWorkspaceArtifactId}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to query {nameof(DestinationWorkspaceTag)} object: {{request}}", request);
					throw new SyncKeplerException($"Failed to query {nameof(DestinationWorkspaceTag)} in workspace {sourceWorkspaceArtifactId}", ex);
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

			int federatedInstanceId = await _federatedInstance.GetInstanceIdAsync().ConfigureAwait(false);

			using (var objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef { Guid = _objectTypeGuid },
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
					throw new SyncKeplerException($"Service call failed while creating {nameof(DestinationWorkspaceTag)}: {request}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to create {nameof(DestinationWorkspaceTag)}: {{request}}", request);
					string tagName = request.FieldValues.First(x => x.Field.Guid == _nameGuid).Value.ToString();
					throw new SyncKeplerException(
						$"Failed to create {nameof(DestinationWorkspaceTag)} '{tagName}' in workspace {sourceWorkspaceArtifactId}",
						ex);
				}

				var createdTag = new DestinationWorkspaceTag
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

			var request = new UpdateRequest
			{
				Object = new RelativityObjectRef { ArtifactID = destinationWorkspaceTag.ArtifactId },
				FieldValues = CreateFieldValues(destinationWorkspaceTag.DestinationWorkspaceArtifactId, destinationWorkspaceTag.DestinationWorkspaceName, federatedInstanceName, federatedInstanceId),
			};

			using (var objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				try
				{
					await objectManager.UpdateAsync(sourceWorkspaceArtifactId, request).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex, $"Service call failed while updating {nameof(DestinationWorkspaceTag)}: {{request}}", request);
					throw new SyncKeplerException($"Failed to update {nameof(DestinationWorkspaceTag)} with id {destinationWorkspaceTag.ArtifactId} in workspace {sourceWorkspaceArtifactId}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to update {nameof(DestinationWorkspaceTag)}: {{request}}", request);
					throw new SyncKeplerException($"Failed to update {nameof(DestinationWorkspaceTag)} with id {destinationWorkspaceTag.ArtifactId} in workspace {sourceWorkspaceArtifactId}", ex);
				}
			}
		}

		public async Task<IList<TagDocumentsResult>> TagDocumentsAsync(ISynchronizationConfiguration synchronizationConfiguration, IList<int> documentArtifactIds, CancellationToken token)
		{
			var tagResults = new List<TagDocumentsResult>();
			if (documentArtifactIds.Count == 0)
			{
				const string noUpdateMessage = "A call to the Mass Update API was not made as there are no objects to update.";
				var result = new TagDocumentsResult(documentArtifactIds, noUpdateMessage, true, documentArtifactIds.Count);
				tagResults.Add(result);
				return tagResults;
			}

			IEnumerable<FieldRefValuePair> fieldValues = GetDocumentFieldTags(synchronizationConfiguration);
			var massUpdateOptions = new MassUpdateOptions
			{
				UpdateBehavior = FieldUpdateBehavior.Merge
			};
			IEnumerable<IList<int>> documentArtifactIdBatches = documentArtifactIds.SplitList(_MAX_OBJECT_QUERY_BATCH_SIZE);
			foreach (IList<int> documentArtifactIdBatch in documentArtifactIdBatches)
			{
				TagDocumentsResult tagResult = await TagDocumentsBatchAsync(synchronizationConfiguration, documentArtifactIdBatch, fieldValues, massUpdateOptions, token).ConfigureAwait(false);
				tagResults.Add(tagResult);
			}

			return tagResults;
		}

		private async Task<TagDocumentsResult> TagDocumentsBatchAsync(
			ISynchronizationConfiguration synchronizationConfiguration, IList<int> batch, IEnumerable<FieldRefValuePair> fieldValues, MassUpdateOptions massUpdateOptions, CancellationToken token)
		{
			var metricsCustomData = new Dictionary<string, object> { { "batchSize", batch.Count } };

			var updateByIdentifiersRequest = new MassUpdateByObjectIdentifiersRequest
			{
				Objects = ConvertArtifactIdsToObjectRefs(batch),
				FieldValues = fieldValues
			};

			TagDocumentsResult result;
			try
			{
				using (_syncMetrics.TimedOperation("Relativity.Sync.TagDocuments.SourceUpdate.Time", ExecutionStatus.None, metricsCustomData))
				using (var objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
				{
					MassUpdateResult updateResult = await objectManager
						.UpdateAsync(synchronizationConfiguration.SourceWorkspaceArtifactId, updateByIdentifiersRequest,
							massUpdateOptions, token).ConfigureAwait(false);
					result = GenerateTagDocumentsResult(updateResult, batch);
				}
			}
			catch (Exception updateException)
			{
				const string exceptionMessage = "Mass tagging Documents with Destination Workspace and Job History fields failed.";
				const string exceptionTemplate =
					"Mass tagging documents in source workspace {SourceWorkspace} with destination workspace field {DestinationWorkspaceField} and job history field {JobHistoryField} failed.";

				_logger.LogError(updateException, exceptionTemplate,
					synchronizationConfiguration.SourceWorkspaceArtifactId, synchronizationConfiguration.DestinationWorkspaceTagArtifactId, synchronizationConfiguration.JobHistoryArtifactId);
				result = new TagDocumentsResult(batch, exceptionMessage, false, 0);
			}

			_syncMetrics.GaugeOperation("Relativity.Sync.TagDocuments.SourceUpdate.Count", ExecutionStatus.None, result.TotalObjectsUpdated, "document(s)", metricsCustomData);

			return result;
		}

		private static IReadOnlyList<RelativityObjectRef> ConvertArtifactIdsToObjectRefs(IList<int> artifactIds)
		{
			var objectRefs = new RelativityObjectRef[artifactIds.Count];

			for (int i = 0; i < artifactIds.Count; i++)
			{
				var objectRef = new RelativityObjectRef
				{
					ArtifactID = artifactIds[i]
				};
				objectRefs[i] = objectRef;
			}
			return objectRefs;
		}

		private FieldRefValuePair[] GetDocumentFieldTags(ISynchronizationConfiguration synchronizationConfiguration)
		{
			FieldRefValuePair[] fieldRefValuePairs =
			{
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = _destinationWorkspaceFieldMultiObject },
					Value = ToMultiObjectValue(synchronizationConfiguration.DestinationWorkspaceTagArtifactId)
				},
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = _jobHistoryFieldMultiObject },
					Value = ToMultiObjectValue(synchronizationConfiguration.JobHistoryArtifactId)
				}
			};
			return fieldRefValuePairs;
		}

		private static IEnumerable<RelativityObjectRef> ToMultiObjectValue(params int[] artifactIds)
		{
			return artifactIds.Select(x => new RelativityObjectRef { ArtifactID = x });
		}

		private static TagDocumentsResult GenerateTagDocumentsResult(MassUpdateResult updateResult, IList<int> batch)
		{
			IEnumerable<int> failedDocumentArtifactIds;
			if (!updateResult.Success)
			{
				int elementsToCapture = batch.Count - updateResult.TotalObjectsUpdated;
				failedDocumentArtifactIds = batch.ToList().GetRange(updateResult.TotalObjectsUpdated, elementsToCapture);
			}
			else
			{
				failedDocumentArtifactIds = Array.Empty<int>();
			}
			var result = new TagDocumentsResult(failedDocumentArtifactIds, updateResult.Message, updateResult.Success, updateResult.TotalObjectsUpdated);
			return result;
		}

		private IEnumerable<FieldRefValuePair> CreateFieldValues(int destinationWorkspaceArtifactId, string destinationWorkspaceName, string federatedInstanceName, int federatedInstanceId)
		{
			string destinationTagName = _tagNameFormatter.FormatWorkspaceDestinationTagName(federatedInstanceName, destinationWorkspaceName, destinationWorkspaceArtifactId);
			FieldRefValuePair[] pairs =
			{
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = _nameGuid },
					Value = destinationTagName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = _destinationWorkspaceNameGuid },
					Value = destinationWorkspaceName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = _destinationWorkspaceArtifactIdGuid },
					Value = destinationWorkspaceArtifactId
				},
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = _destinationInstanceNameGuid },
					Value = federatedInstanceName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = _destinationInstanceArtifactIdGuid },
					Value = federatedInstanceId
				}
			};

			return pairs;
		}
	}
}