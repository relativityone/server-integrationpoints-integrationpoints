using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class RelativitySourceCaseTagRepository : IRelativitySourceCaseTagRepository
	{
		private readonly IDestinationServiceFactoryForUser _serviceFactoryForUser;
		private readonly ISyncLog _logger;

		private static readonly Guid ObjectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
		private static readonly Guid CaseIdFieldNameGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
		private static readonly Guid CaseNameFieldNameGuid = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");
		private static readonly Guid InstanceNameFieldGuid = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
		private static readonly Guid SourceWorkspaceNameGuid = new Guid("A16F7BEB-B3B0-4658-BB52-1C801BA920F0");

		public RelativitySourceCaseTagRepository(IDestinationServiceFactoryForUser serviceFactoryForUser, ISyncLog logger)
		{
			_serviceFactoryForUser = serviceFactoryForUser;
			_logger = logger;
		}

		public async Task<RelativitySourceCaseTag> ReadAsync(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactTypeId, 
			int sourceWorkspaceArtifactId, string sourceInstanceName, CancellationToken token)
		{
			RelativityObject tag = await QueryRelativityObjectTagAsync(destinationWorkspaceArtifactId, sourceWorkspaceArtifactId, sourceInstanceName).ConfigureAwait(false);

			if (tag != null)
			{
				RelativitySourceCaseTag sourceCaseTag = new RelativitySourceCaseTag()
				{

				};
				return sourceCaseTag;
			}

			return null;
		}

		private async Task<RelativityObject> QueryRelativityObjectTagAsync(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactId, string sourceInstanceName)
		{
			using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest()
				{
					Condition = $"('{CaseIdFieldNameGuid}' == {sourceWorkspaceArtifactId}) AND ('{InstanceNameFieldGuid}' == {sourceInstanceName})",
					ObjectType = new ObjectTypeRef()
					{
						Guid = ObjectTypeGuid
					},
					Fields = new List<FieldRef>()
					{
						new FieldRef(){Guid = CaseIdFieldNameGuid},
						new FieldRef(){Guid = CaseNameFieldNameGuid},
						new FieldRef(){Guid = SourceWorkspaceNameGuid},
						new FieldRef(){Guid = InstanceNameFieldGuid}
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
					_logger.LogError(ex, $"Service call failed while querying {nameof(RelativitySourceCaseTag)} object: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Service call failed while querying {nameof(RelativitySourceCaseTag)} in workspace {destinationWorkspaceArtifactId}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to query {nameof(RelativitySourceCaseTag)} object: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Failed to query {nameof(RelativitySourceCaseTag)} in workspace {destinationWorkspaceArtifactId}", ex);
				}

				return queryResult.Objects.FirstOrDefault();
			}
		}

		public async Task<RelativitySourceCaseTag> CreateAsync(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactTypeId, RelativitySourceCaseTag sourceCaseTag)
		{
			using (IObjectManager objectManager = await _serviceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				CreateRequest request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = sourceWorkspaceArtifactTypeId
					},
					FieldValues = CreateFields(sourceCaseTag.Name, sourceCaseTag.SourceWorkspaceArtifactId, sourceCaseTag.SourceWorkspaceName, sourceCaseTag.SourceInstanceName)
				};

				CreateResult createResult;
				try
				{
					createResult = await objectManager.CreateAsync(destinationWorkspaceArtifactId, request).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex, $"Service call failed while creating {nameof(RelativitySourceCaseTag)}: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Service call failed while creating {nameof(RelativitySourceCaseTag)}: {request}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to create {nameof(RelativitySourceCaseTag)}: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Failed to create {nameof(RelativitySourceCaseTag)} '{sourceCaseTag.Name}' in workspace {destinationWorkspaceArtifactId}",
						ex);
				}

				RelativitySourceCaseTag createdTag = new RelativitySourceCaseTag()
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

		private List<FieldRefValuePair> CreateFields(string tagName, int sourceWorkspaceArtifactId, string sourceWorkspaceName, string instanceName)
		{
			return new List<FieldRefValuePair>
			{
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = CaseNameFieldNameGuid},
					Value = tagName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = CaseIdFieldNameGuid},
					Value = sourceWorkspaceArtifactId
				},
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = SourceWorkspaceNameGuid },
					Value = sourceWorkspaceName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef { Guid = InstanceNameFieldGuid },
					Value = instanceName
				}
			};
		}

		public async Task<RelativitySourceCaseTag> UpdateAsync(int sourceWorkspaceArtifactTypeId, RelativitySourceCaseTag sourceCaseTag, CancellationToken token)
		{
			throw new System.NotImplementedException();
		}
	}
}