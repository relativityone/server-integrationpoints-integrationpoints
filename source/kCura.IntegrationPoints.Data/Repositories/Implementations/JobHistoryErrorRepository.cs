using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Commands.MassEdit;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.API;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class JobHistoryErrorRepository : RelativityMassEditBase, IJobHistoryErrorRepository
	{
		private readonly IGenericLibrary<JobHistoryError> _jobHistoryErrorLibrary;
		private readonly IDtoTransformer<JobHistoryErrorDTO, JobHistoryError> _dtoTransformer;
		private readonly IHelper _helper;

		public JobHistoryErrorRepository(IHelper helper, int workspaceArtifactId)
			: this(new RsapiClientLibrary<JobHistoryError>(helper, workspaceArtifactId),
				  new JobHistoryErrorTransformer(helper, workspaceArtifactId))
		{
			_helper = helper;
		}

		/// <summary>
		/// To be used externally by unit tests only
		/// </summary>
		internal JobHistoryErrorRepository(IGenericLibrary<JobHistoryError> jobHistoryErrorLibrary, IDtoTransformer<JobHistoryErrorDTO, JobHistoryError> dtoTransformer)
		{
			_jobHistoryErrorLibrary = jobHistoryErrorLibrary;
			_dtoTransformer = dtoTransformer;
		}

		public void UpdateErrorStatuses(ClaimsPrincipal claimsPrincipal, int sourceWorkspaceId, Relativity.Client.Choice errorStatus, string tableSuffix)
		{
		}

		public List<JobHistoryError> RetreiveJobHistoryErrors(int jobHistoryArtifactId)
		{
			return new List<JobHistoryError>();
		}

		public int CreateItemLevelErrorsSavedSearch(int workspaceArtifactId, int savedSearchArtifactId, int jobHistoryArtifactId)
		{
			FieldRef savedSearchFieldRef = new FieldRef("(Saved Search)");
			Criteria savedSearchCriteria = new Criteria
			{
				Condition = new CriteriaCondition(savedSearchFieldRef, CriteriaConditionEnum.In, savedSearchArtifactId),
				BooleanOperator = BooleanOperatorEnum.And
			};

			FieldRef jobHistoryArtfiactIdFieldRef = new FieldRef("Job History");
			Criteria jobHistoryArtifactIdCriteria = new Criteria
			{
				Condition = new CriteriaCondition(jobHistoryArtfiactIdFieldRef, CriteriaConditionEnum.AnyOfThese, new[] { jobHistoryArtifactId }) { NotOperator = true }
			};
			CriteriaCollection jobHistoryObjectCriteriaCollection = new CriteriaCollection
			{
				Conditions = new List<CriteriaBase>(1) { jobHistoryArtifactIdCriteria }
			};
			
			FieldRef jobHistoryFieldRef = new FieldRef("Job History");
			Criteria jobHistoryCriteria = new Criteria
			{
				Condition = new CriteriaCondition(jobHistoryFieldRef, CriteriaConditionEnum.In, jobHistoryObjectCriteriaCollection)
			};

			CriteriaCollection searchCondition = new CriteriaCollection
			{
				Conditions = new List<CriteriaBase>(2) { savedSearchCriteria, jobHistoryCriteria }
			};

			KeywordSearch itemLevelSearch = new KeywordSearch
			{
				Name = "Temporary Search",
				ArtifactTypeID = (int)Relativity.Client.ArtifactType.Document,
				SearchCriteria = searchCondition
			};

			using (IKeywordSearchManager searchManager = _helper.GetServicesManager().CreateProxy<IKeywordSearchManager>(ExecutionIdentity.CurrentUser))
			{
				SearchResultViewFields fields = searchManager.GetFieldsForSearchResultViewAsync(workspaceArtifactId, (int)Relativity.Client.ArtifactType.Document)
					.ConfigureAwait(false).GetAwaiter().GetResult();

				FieldRef field = fields.FieldsNotIncluded.First(x => x.Name == "Artifact ID");
				itemLevelSearch.Fields = new List<FieldRef>(1) { field };
				
				int itemLevelSearchArtifactId =
					searchManager.CreateSingleAsync(workspaceArtifactId, itemLevelSearch)
						.ConfigureAwait(false).GetAwaiter().GetResult();

				return itemLevelSearchArtifactId;
			}
		}
	}
}