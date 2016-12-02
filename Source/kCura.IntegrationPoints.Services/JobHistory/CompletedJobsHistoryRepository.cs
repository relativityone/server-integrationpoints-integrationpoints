using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.QueryBuilders;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Services.JobHistory
{
	public class CompletedJobsHistoryRepository : ICompletedJobsHistoryRepository
	{
		private readonly ILibraryFactory _libraryFactory;
		private readonly IIntegrationPointsCompletedJobsQueryBuilder _integrationPointsCompletedJobsQueryBuilder;

		public CompletedJobsHistoryRepository(ILibraryFactory libraryFactory, IIntegrationPointsCompletedJobsQueryBuilder integrationPointsCompletedJobsQueryBuilder)
		{
			_libraryFactory = libraryFactory;
			_integrationPointsCompletedJobsQueryBuilder = integrationPointsCompletedJobsQueryBuilder;
		}

		public IList<JobHistoryModel> RetrieveCompleteJobsForIntegrationPoints(JobHistoryRequest request, List<IntegrationPoint> integrationPoints)
		{
			var sortDescending = (request.SortDescending != null) && request.SortDescending.Value;
			var integrationPointsIds = integrationPoints.Select(x => x.ArtifactId).ToList();
			Query<RDO> query = _integrationPointsCompletedJobsQueryBuilder.CreateQuery(request.SortColumnName, sortDescending, integrationPointsIds);

			IGenericLibrary<Data.JobHistory> library = _libraryFactory.Create<Data.JobHistory>(request.WorkspaceArtifactId);
			IList<Data.JobHistory> queryResult = library.Query(query);

			return queryResult.Select(x => new JobHistoryModel
			{
				ItemsTransferred = x.ItemsTransferred ?? 0,
				DestinationWorkspace = x.DestinationWorkspace,
				EndTimeUTC = x.EndTimeUTC.GetValueOrDefault()
			}).ToList();
		}
	}
}