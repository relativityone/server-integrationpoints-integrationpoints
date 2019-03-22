using System;
using System.Collections.Generic;
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
	internal sealed class RelativitySourceJobTagRepository : IRelativitySourceJobTagRepository
	{
		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly ITagNameFormatter _tagNameFormatter;
		private readonly ISyncLog _logger;

		private static readonly Guid JobHistoryNameGuid = new Guid("07061466-5fab-4581-979c-c801e8207370");
		private static readonly Guid JobHistoryIdFieldGuid = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
		private static readonly Guid JobNameFieldGuid = Guid.NewGuid(); // TODO

		public RelativitySourceJobTagRepository(ISourceServiceFactoryForUser sourceServiceFactoryForUser, ITagNameFormatter tagNameFormatter, ISyncLog logger)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_tagNameFormatter = tagNameFormatter;
			_logger = logger;
		}

		public async Task<RelativitySourceJobTag> CreateAsync(int destinationWorkspaceArtifactId, RelativitySourceJobTag sourceJobTag, CancellationToken token)
		{
			string jobHistoryName = await GetJobHistoryNameAsync(sourceJobTag.SourceCaseTagArtifactId, sourceJobTag.ArtifactId, token).ConfigureAwait(false);
			string tagName = _tagNameFormatter.FormatSourceJobTagName(jobHistoryName, sourceJobTag.JobHistoryArtifactId);

			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				CreateRequest request = new CreateRequest()
				{
					ObjectType = new ObjectTypeRef()
					{
						ArtifactTypeID = sourceJobTag.ArtifactTypeId
					},
					FieldValues = CreateFieldValues(tagName, sourceJobTag.ArtifactId, jobHistoryName)
				};

				CreateResult result;
				try
				{
					result = await objectManager.CreateAsync(destinationWorkspaceArtifactId, request).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex, $"Service call failed while creating {nameof(RelativitySourceJobTag)}: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Service call failed while creating {nameof(RelativitySourceJobTag)}: {request}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to create {nameof(RelativitySourceJobTag)}: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Failed to create {nameof(RelativitySourceJobTag)} '{tagName}' in workspace {sourceJobTag.SourceCaseTagArtifactId}",
						ex);
				}

				RelativitySourceJobTag createdTag = new RelativitySourceJobTag()
				{
					ArtifactId = result.Object.ArtifactID,
					ArtifactTypeId = sourceJobTag.ArtifactTypeId,
					Name = tagName,
					SourceCaseTagArtifactId = sourceJobTag.SourceCaseTagArtifactId,
					JobHistoryArtifactId = sourceJobTag.JobHistoryArtifactId,
					JobHistoryName = jobHistoryName
				};
				
				return createdTag;
			}
		}

		private async Task<string> GetJobHistoryNameAsync(int sourceWorkspaceArtifactId, int jobArtifactId, CancellationToken token)
		{
			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				QueryRequest request = new QueryRequest()
				{
					ObjectType = new ObjectTypeRef() { ArtifactID = jobArtifactId },
					Fields = new []
					{
						new FieldRef()
						{
							Guid = JobHistoryNameGuid
						}
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
					_logger.LogError(ex, $"Service call failed while querying job history name object: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Service call failed while querying job history name in workspace {sourceWorkspaceArtifactId}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to query job history name object: {{request}}", request);
					throw new DestinationWorkspaceTagRepositoryException($"Failed to query job history name in workspace {sourceWorkspaceArtifactId}", ex);
				}

				RelativityObject relativityObject = queryResult.Objects.FirstOrDefault();
				return relativityObject?.Name;
			}
		}

		private IEnumerable<FieldRefValuePair> CreateFieldValues(string tagName, int sourceJobArtifactTypeId, string jobHistoryName)
		{
			FieldRefValuePair[] pairs =
			{
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = JobNameFieldGuid},
					Value = tagName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = JobHistoryIdFieldGuid},
					Value = sourceJobArtifactTypeId
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = JobHistoryNameGuid},
					Value = jobHistoryName
				}
			};

			return pairs;
		}
	}
}