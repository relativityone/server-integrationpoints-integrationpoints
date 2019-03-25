using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class RelativitySourceJobTagRepository : IRelativitySourceJobTagRepository
	{
		private readonly ISourceServiceFactoryForUser _sourceServiceFactoryForUser;
		private readonly ISyncLog _logger;

		private static readonly Guid JobHistoryNameGuid = new Guid("0b8fcebf-4149-4f1b-a8bc-d88ff5917169");
		private static readonly Guid JobHistoryIdFieldGuid = new Guid("2bf54e79-7f75-4a51-a99a-e4d68f40a231");
		private static readonly Guid RelativitySourceJobTypeGuid = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");

		public RelativitySourceJobTagRepository(ISourceServiceFactoryForUser sourceServiceFactoryForUser, ISyncLog logger)
		{
			_sourceServiceFactoryForUser = sourceServiceFactoryForUser;
			_logger = logger;
		}

		public async Task<RelativitySourceJobTag> CreateAsync(int destinationWorkspaceArtifactId, RelativitySourceJobTag sourceJobTag, CancellationToken token)
		{
			_logger.LogVerbose($"Creating {nameof(RelativitySourceJobTag)} in destination workspace artifact ID: {{destinationWorkspaceArtifactId}} Source case tah artifact ID: {{sourceCaseTagArtifactId}}",
				destinationWorkspaceArtifactId, sourceJobTag.SourceCaseTagArtifactId);
			using (IObjectManager objectManager = await _sourceServiceFactoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				CreateRequest request = new CreateRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = RelativitySourceJobTypeGuid,
					},
					ParentObject = new RelativityObjectRef {ArtifactID = sourceJobTag.SourceCaseTagArtifactId},
					FieldValues = CreateFieldValues(sourceJobTag.Name, sourceJobTag.JobHistoryArtifactId, sourceJobTag.JobHistoryName)
				};

				CreateResult result;
				try
				{
					result = await objectManager.CreateAsync(destinationWorkspaceArtifactId, request).ConfigureAwait(false);
				}
				catch (ServiceException ex)
				{
					_logger.LogError(ex, $"Service call failed while creating {nameof(RelativitySourceJobTag)}: {{request}}", request);
					throw new RelativitySourceJobTagRepositoryException($"Service call failed while creating {nameof(RelativitySourceJobTag)}: {request}", ex);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to create {nameof(RelativitySourceJobTag)}: {{request}}", request);
					throw new RelativitySourceJobTagRepositoryException($"Failed to create {nameof(RelativitySourceJobTag)} '{sourceJobTag.Name}' in workspace {sourceJobTag.SourceCaseTagArtifactId}",
						ex);
				}

				RelativitySourceJobTag createdTag = new RelativitySourceJobTag()
				{
					ArtifactId = result.Object.ArtifactID,
					ArtifactTypeId = sourceJobTag.ArtifactTypeId,
					Name = sourceJobTag.Name,
					SourceCaseTagArtifactId = sourceJobTag.SourceCaseTagArtifactId,
					JobHistoryArtifactId = sourceJobTag.JobHistoryArtifactId,
					JobHistoryName = sourceJobTag.JobHistoryName
				};
				
				return createdTag;
			}
		}

		private IEnumerable<FieldRefValuePair> CreateFieldValues(string sourceJobTagName, int jobHistoryArtifactId, string jobHistoryName)
		{
			FieldRefValuePair[] pairs =
			{
				new FieldRefValuePair
				{
					Field = new FieldRef {Name = "Name"},
					Value = sourceJobTagName
				},
				new FieldRefValuePair
				{
					Field = new FieldRef {Guid = JobHistoryIdFieldGuid},
					Value = jobHistoryArtifactId
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