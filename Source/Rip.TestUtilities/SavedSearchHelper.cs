using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace Rip.TestUtilities
{
	public class SavedSearchHelper
	{
		private readonly int _workspaceID;
		private readonly IKeywordSearchManager _keywordSearchManager;

		public SavedSearchHelper(int workspaceID, IKeywordSearchManager keywordSearchManager)
		{
			_workspaceID = workspaceID;
			_keywordSearchManager = keywordSearchManager;
		}

		public int CreateSavedSearch(DocumentsTestData documentsTestData)
		{
			string savedSearchName = Guid.NewGuid().ToString();
			int searchArtifactID = SavedSearch.CreateSavedSearch(_workspaceID, savedSearchName);

			var controlNumberFieldRef = new FieldRef { Name = TestConstants.FieldNames.CONTROL_NUMBER };
			var controlNumbers = documentsTestData.AllDocumentsDataTable
				.AsEnumerable()
				.Select(x => x[TestConstants.FieldNames.CONTROL_NUMBER])
				.ToArray();

			List<CriteriaBase> conditions = controlNumbers.Select(x => new Criteria
			{
				Condition = new CriteriaCondition(controlNumberFieldRef, CriteriaConditionEnum.Is, x),
				BooleanOperator = BooleanOperatorEnum.Or
			}).Cast<CriteriaBase>().ToList();

			CriteriaCollection savedSearchCriteriaCollection = new CriteriaCollection
			{
				Conditions = conditions
			};

			SavedSearch.UpdateSavedSearchCriteria(_workspaceID, searchArtifactID, savedSearchCriteriaCollection);

			return searchArtifactID;
		}

		public int CreateSavedSearch(string fieldName, CriteriaConditionEnum criteriaCondition, object fieldValue)
		{
			string savedSearchName = Guid.NewGuid().ToString();
			int searchArtifactID = SavedSearch.CreateSavedSearch(_workspaceID, savedSearchName);

			var fieldRef = new FieldRef { Name = fieldName };
			var conditions = new List<CriteriaBase>
			{
				new Criteria {Condition = new CriteriaCondition(fieldRef, criteriaCondition, fieldValue)}
			};

			CriteriaCollection savedSearchCriteriaCollection = new CriteriaCollection
			{
				Conditions = conditions
			};

			SavedSearch.UpdateSavedSearchCriteria(_workspaceID, searchArtifactID, savedSearchCriteriaCollection);

			return searchArtifactID;
		}

		public void DeleteSavedSearch(int searchArtifactID)
		{
			_keywordSearchManager.DeleteSingleAsync(_workspaceID, searchArtifactID);
		}
	}
}
