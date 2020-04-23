using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Services.Field;
using Relativity.Services.Search;
using Relativity.Services.ServiceProxy;

namespace Relativity.Sync.WorkspaceGenerator.SavedSearch
{
	public class SavedSearchManager : ISavedSearchManager
	{
		private readonly Guid _fileIconGuid = new Guid("861295b5-5b1d-4830-89e7-77e0a7ef1c30");
		private readonly Guid _controlNumberGuid = new Guid("2a3f1212-c8ca-4fa9-ad6b-f76c97f05438");

		private readonly IServiceFactory _serviceFactory;

		public SavedSearchManager(IServiceFactory serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task CreateSavedSearchForTestCaseAsync(int workspaceID, string testCaseName)
		{
			using (var keywordSearchManager = _serviceFactory.CreateProxy<IKeywordSearchManager>())
			{
				KeywordSearch search = CreateSavedSearchDTO(testCaseName);
				await keywordSearchManager.CreateSingleAsync(workspaceID, search).ConfigureAwait(false);
			}
		}

		private KeywordSearch CreateSavedSearchDTO(string testCaseName)
		{
			FieldRef controlNumberField = new FieldRef(new List<Guid>(){_controlNumberGuid});
			FieldRef fileIconField = new FieldRef(new List<Guid>(){_fileIconGuid});

			CriteriaCollection criteria = new CriteriaCollection();
			criteria.Conditions.Add(new Criteria()
			{
				Condition = new CriteriaCondition(controlNumberField, CriteriaConditionEnum.StartsWith, $"{testCaseName}{Consts.ControlNumberSeparator}")
			});

			KeywordSearch search = new KeywordSearch()
			{
				Name = testCaseName,
				ArtifactTypeID = (int)ArtifactType.Document,
				SearchCriteria = criteria,
				SearchContainer = new SearchContainerRef(),
				Fields = new List<FieldRef>()
				{
					fileIconField,
					controlNumberField
				}
			};
			return search;
		}
	}
}