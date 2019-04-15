using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Field;
using Relativity.Services.Search;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Executors
{
	internal sealed class TagSavedSearch : ITagSavedSearch
	{
		private readonly Guid _jobHistoryFieldOnDocumentGuid = new Guid("7cc3faaf-cbb8-4315-a79f-3aa882f1997f");
		private readonly Guid _sourceWorkspaceFieldOnDocumentGuid = new Guid("2fa844e3-44f0-47f9-abb7-d6d8be0c9b8f");
		private readonly Guid _fileIconGuid = new Guid("861295b5-5b1d-4830-89e7-77e0a7ef1c30");
		private readonly Guid _controlNumberGuid = new Guid("2a3f1212-c8ca-4fa9-ad6b-f76c97f05438");

		private readonly IDestinationServiceFactoryForUser _destinationServiceFactoryForUser;
		private readonly ISyncLog _syncLog;

		public TagSavedSearch(IDestinationServiceFactoryForUser destinationServiceFactoryForUser, ISyncLog syncLog) //, IMultiObjectSavedSearchCondition multiObjectSavedSearchCondition, ISyncLog syncLog)
		{
			_destinationServiceFactoryForUser = destinationServiceFactoryForUser;
			//_multiObjectSavedSearchCondition = multiObjectSavedSearchCondition;
			_syncLog = syncLog;
		}

		public async Task<int> CreateTagSavedSearchAsync(IDestinationWorkspaceSavedSearchCreationConfiguration configuration, int savedSearchFolderArtifactId, CancellationToken token)
		{
			int destinationWorkspaceArtifactId = configuration.DestinationWorkspaceArtifactId;

			try
			{
				using (var keywordSearchManager = await _destinationServiceFactoryForUser.CreateProxyAsync<IKeywordSearchManager>().ConfigureAwait(false))
				{
					KeywordSearch searchDto = CreateKeywordSearchForTagging(null, savedSearchFolderArtifactId);
					int keywordSearchId = await keywordSearchManager.CreateSingleAsync(destinationWorkspaceArtifactId, searchDto).ConfigureAwait(false);

					_syncLog.LogDebug("Created tagging keyword search: {keywordSearchId}", keywordSearchId);
					return keywordSearchId;
				}
			}
			catch (Exception e)
			{
				_syncLog.LogError(e, "Failed to create Saved Search for promoted documents in destination workspace {workspaceArtifactId}.", destinationWorkspaceArtifactId);
				throw new DestinationWorkspaceTagRepositoryException($"Failed to create Saved Search for promoted documents in destination workspace {destinationWorkspaceArtifactId}.", e);
			}
		}

		private KeywordSearch CreateKeywordSearchForTagging(IDestinationWorkspaceSavedSearchCreationConfiguration configuration, int savedSearchFolderId)
		{
			CriteriaCollection criteria = CreateWorkspaceAndJobCriteria(configuration);

			return new KeywordSearch
			{
				Name = LimitLength(configuration.SourceJobTagName),
				ArtifactTypeID = (int)ArtifactType.Document,
				SearchCriteria = criteria,
				SearchContainer = new SearchContainerRef(savedSearchFolderId),
				Fields = new List<FieldRef>
				{
					new FieldRef(new List<Guid> { _fileIconGuid }),
					new FieldRef("Edit"),
					new FieldRef(new List<Guid> { _controlNumberGuid })
				}
			};
		}

		private static string LimitLength(string name)
		{
			const int two = 2;
			const int three = 3;

			const int maxLength = 50;
			return name.Length > maxLength
				? name.Remove(maxLength / two - three) + "..." + name.Substring(name.Length - maxLength / two)
				: name;
		}

		private CriteriaCollection CreateWorkspaceAndJobCriteria(IDestinationWorkspaceSavedSearchCreationConfiguration configuration)
		{
			var conditionCollection = new CriteriaCollection();

			CriteriaBase sourceJobCriteria = CreateConditionForMultiObject(
				_jobHistoryFieldOnDocumentGuid, CriteriaConditionEnum.AllOfThese, new List<int> { configuration.SourceJobTagArtifactId });

			CriteriaBase sourceWorkspaceCriteria = CreateConditionForMultiObject(
				_sourceWorkspaceFieldOnDocumentGuid, CriteriaConditionEnum.AllOfThese, new List<int> { 1 });	// TODO: source workspace artifact ID

			conditionCollection.Conditions.Add(sourceJobCriteria);
			conditionCollection.Conditions.Add(sourceWorkspaceCriteria);

			return conditionCollection;
		}

		private CriteriaBase CreateConditionForMultiObject(Guid fieldGuid, CriteriaConditionEnum conditionEnum, List<int> values)
		{
			var fieldIdentifier = new FieldRef(new List<Guid> { fieldGuid });

			// Create main condition
			var criteria = new Criteria
			{
				Condition = new CriteriaCondition(fieldIdentifier, conditionEnum, values),
				BooleanOperator = BooleanOperatorEnum.And
			};

			// Aggregate condition with CriteriaCollection
			var criteriaCollection = new CriteriaCollection();
			criteriaCollection.Conditions.Add(criteria);

			// Multi-objects require condition to be aggregated into additional CriteriaCondition
			var parentCriteria = new Criteria
			{
				Condition = new CriteriaCondition(fieldIdentifier, CriteriaConditionEnum.In, criteriaCollection)
			};
			return parentCriteria;
		}
	}
}