using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class RelativitySourceCaseTagRepository : IRelativitySourceCaseTagRepository
	{
		private const string _NAME_FIELD_NAME = "Name";
		private const string _SENSITIVE_DATA_REMOVED = "[Sensitive user data has been removed]";

		private readonly IDestinationServiceFactoryForUser _serviceFactoryForUser;
		private readonly IAPILog _logger;

		private static readonly Guid ObjectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
		private static readonly Guid CaseIdFieldGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
		private static readonly Guid InstanceNameFieldGuid = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
		private static readonly Guid SourceWorkspaceNameFieldGuid = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");

		public RelativitySourceCaseTagRepository(IDestinationServiceFactoryForUser serviceFactoryForUser, IAPILog logger)
		{
			_serviceFactoryForUser = serviceFactoryForUser;
			_logger = logger;
		}

		public async Task<RelativitySourceCaseTag> ReadAsync(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactId, string sourceInstanceName, CancellationToken token)
		{
			_logger.LogVerbose(
				"Reading {tagName}. Source workspace artifact ID: {sourceWorkspaceArtifactId} Destination workspace artifact ID: {destinationWorkspaceArtifactId}",
				nameof(RelativitySourceCaseTag), sourceWorkspaceArtifactId, destinationWorkspaceArtifactId);
			RelativityObject tag = await QueryRelativityObjectTagAsync(destinationWorkspaceArtifactId, sourceWorkspaceArtifactId, sourceInstanceName, token).ConfigureAwait(false);

			if (tag != null)
			{
				RelativitySourceCaseTag sourceCaseTag = new RelativitySourceCaseTag
				{
					ArtifactId = tag.ArtifactID,
					Name = tag.Name,
					SourceInstanceName = tag[InstanceNameFieldGuid].Value.ToString(),
					SourceWorkspaceName = tag[SourceWorkspaceNameFieldGuid].Value.ToString(),
					SourceWorkspaceArtifactId = Convert.ToInt32(tag[CaseIdFieldGuid].Value, CultureInfo.InvariantCulture)
				};
				return sourceCaseTag;
			}

			return null;
		}

		private async Task<RelativityObject> QueryRelativityObjectTagAsync(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactId, string sourceInstanceName, CancellationToken token)
		{
			using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest
				{
					Condition = $"('{CaseIdFieldGuid}' == {sourceWorkspaceArtifactId}) AND ('{InstanceNameFieldGuid}' == '{sourceInstanceName}')",
					ObjectType = new ObjectTypeRef
					{
						Guid = ObjectTypeGuid
					},
					IncludeNameInQueryResult = true,
					Fields = new List<FieldRef>
					{
						new FieldRef {Guid = CaseIdFieldGuid},
						new FieldRef {Guid = SourceWorkspaceNameFieldGuid},
						new FieldRef {Guid = InstanceNameFieldGuid}
					}
				};

				QueryResult queryResult;
				try
				{
					const int start = 0;
					const int length = 1;
					queryResult = await objectManager.QueryAsync(destinationWorkspaceArtifactId, request, start, length).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex, "Service call failed while querying {tagName} object: {request}", nameof(RelativitySourceCaseTag), request);
					throw new RelativitySourceCaseTagRepositoryException($"Service call failed while querying {nameof(RelativitySourceCaseTag)} in workspace {destinationWorkspaceArtifactId}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to query {tagName} object: {request}", nameof(RelativitySourceCaseTag), request);
					throw new RelativitySourceCaseTagRepositoryException($"Failed to query {nameof(RelativitySourceCaseTag)} in workspace {destinationWorkspaceArtifactId}", ex);
				}

				return queryResult.Objects.FirstOrDefault();
			}
		}

		public async Task<RelativitySourceCaseTag> CreateAsync(int destinationWorkspaceArtifactId, RelativitySourceCaseTag sourceCaseTag)
		{
			_logger.LogVerbose("Creating {tagName} in destination workspace artifact ID: {destinationWorkspaceArtifactId}", nameof(RelativitySourceCaseTag), destinationWorkspaceArtifactId);
			using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				CreateRequest request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = ObjectTypeGuid
					},
					FieldValues = CreateFieldValues(sourceCaseTag.Name, sourceCaseTag.SourceWorkspaceArtifactId, sourceCaseTag.SourceWorkspaceName, sourceCaseTag.SourceInstanceName)
				};

				CreateResult createResult;
				try
				{
					createResult = await objectManager.CreateAsync(destinationWorkspaceArtifactId, request).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					request.FieldValues = RemoveSensitiveUserData(request.FieldValues);
					_logger.LogError(ex, "Service call failed while creating {tagName}: {request}", nameof(RelativitySourceCaseTag), request);
					throw new RelativitySourceCaseTagRepositoryException($"Service call failed while creating {nameof(RelativitySourceCaseTag)}: {request}", ex);
				}
				catch (Exception ex)
				{
					request.FieldValues = RemoveSensitiveUserData(request.FieldValues);
					_logger.LogError(ex, "Failed to create {tagName}: {request}", nameof(RelativitySourceCaseTag), request);
					throw new RelativitySourceCaseTagRepositoryException($"Failed to create {nameof(RelativitySourceCaseTag)} in workspace {destinationWorkspaceArtifactId}", ex);
				}

				RelativitySourceCaseTag createdTag = new RelativitySourceCaseTag
				{
					ArtifactId = createResult.Object.ArtifactID,
					Name = sourceCaseTag.Name,
					SourceInstanceName = sourceCaseTag.SourceInstanceName,
					SourceWorkspaceArtifactId = sourceCaseTag.SourceWorkspaceArtifactId,
					SourceWorkspaceName = sourceCaseTag.SourceWorkspaceName
				};
				return createdTag;
			}
		}

		public async Task UpdateAsync(int destinationWorkspaceArtifactId, RelativitySourceCaseTag sourceCaseTag)
		{
			_logger.LogVerbose("Updating {tagName} in destination workspace artifact ID: {destinationWorkspaceArtifactId}", nameof(RelativitySourceCaseTag), destinationWorkspaceArtifactId);
			UpdateRequest request = new UpdateRequest
			{
				Object = new RelativityObjectRef {ArtifactID = sourceCaseTag.ArtifactId},
				FieldValues = CreateFieldValues(sourceCaseTag.Name, sourceCaseTag.SourceWorkspaceArtifactId, sourceCaseTag.SourceWorkspaceName, sourceCaseTag.SourceInstanceName)
			};

			using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				try
				{
					await objectManager.UpdateAsync(destinationWorkspaceArtifactId, request).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					request.FieldValues = RemoveSensitiveUserData(request.FieldValues);
					_logger.LogError(ex, "Service call failed while updating {tagName}: {request}", nameof(RelativitySourceCaseTag), request);
					throw new RelativitySourceCaseTagRepositoryException(
						$"Failed to update {nameof(RelativitySourceCaseTag)} with id {sourceCaseTag.ArtifactId} in workspace {destinationWorkspaceArtifactId}",
						ex);
				}
				catch (Exception ex)
				{
					request.FieldValues = RemoveSensitiveUserData(request.FieldValues);
					_logger.LogError(ex, "Failed to update {tagName}: {request}", nameof(RelativitySourceCaseTag), request);
					throw new RelativitySourceCaseTagRepositoryException(
						$"Failed to update {nameof(RelativitySourceCaseTag)} with id {sourceCaseTag.ArtifactId} in workspace {destinationWorkspaceArtifactId}",
						ex);
				}
			}
		}

		private IEnumerable<FieldRefValuePair> CreateFieldValues(string sourceTagName, int sourceWorkspaceArtifactId, string sourceWorkspaceName, string instanceName)
		{
			FieldRefValuePair[] pairs =
			{
				new FieldRefValuePair
				{
					Field = new FieldRef {Name = _NAME_FIELD_NAME},
					Value = sourceTagName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = CaseIdFieldGuid},
					Value = sourceWorkspaceArtifactId
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = SourceWorkspaceNameFieldGuid},
					Value = sourceWorkspaceName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = InstanceNameFieldGuid},
					Value = instanceName
				}
			};

			return pairs;
		}

		private IEnumerable<FieldRefValuePair> RemoveSensitiveUserData(IEnumerable<FieldRefValuePair> fieldValues)
		{
			fieldValues.First(fieldValue => fieldValue.Field.Name == _NAME_FIELD_NAME).Value = _SENSITIVE_DATA_REMOVED;
			fieldValues.First(fieldValue => fieldValue.Field.Guid == SourceWorkspaceNameFieldGuid).Value = _SENSITIVE_DATA_REMOVED;
			fieldValues.First(fieldValue => fieldValue.Field.Guid == InstanceNameFieldGuid).Value = _SENSITIVE_DATA_REMOVED;

			return fieldValues;
		}
	}
}
