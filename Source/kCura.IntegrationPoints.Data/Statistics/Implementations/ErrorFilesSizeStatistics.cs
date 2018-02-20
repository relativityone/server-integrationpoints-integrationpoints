using System;
using System.Data;
using System.Data.SqlClient;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
	class ErrorFilesSizeStatistics : IErrorFilesSizeStatistics
	{
		private readonly IHelper _helper;
		private readonly IAPILog _logger;

		private const string _FOR_JOBHISTORY_ERROR = "Failed to retrieve total files size for job history id: {JobHistoryArtifactId}.";

		public ErrorFilesSizeStatistics(IHelper helper)
		{
			_helper = helper;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<ErrorFilesSizeStatistics>();
		}
		
		public long ForJobHistoryOmmitedFiles(int workspaceArtifactId, int jobHistoryArtifactId)
		{
			try
			{
				const string sqlText =
					@"	SELECT COALESCE(SUM([Size]),0)
					FROM	[ExtendedArtifact] EA
							JOIN [JobHistoryError] JHE ON EA.ArtifactID = JHE.ArtifactID
							JOIN [Document] D ON JHE.SourceUniqueID = D.ControlNumber
							JOIN [File] F ON D.ArtifactID = F.DocumentArtifactID
					WHERE	[ParentArtifactID] = @artifactId";

				var fileTypeParameter = new SqlParameter("@artifactId", SqlDbType.Int)
				{
					Value = jobHistoryArtifactId
				};

				IDBContext dbContext = _helper.GetDBContext(workspaceArtifactId);
				return dbContext.ExecuteSqlStatementAsScalar<long>(sqlText, fileTypeParameter);
			}
			catch (Exception e)
			{
				_logger.LogError(e, _FOR_JOBHISTORY_ERROR, jobHistoryArtifactId);
				return 0L;
			}
		}
	}
}
