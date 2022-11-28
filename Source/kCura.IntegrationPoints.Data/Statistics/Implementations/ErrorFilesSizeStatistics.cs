using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoints.Data.DbContext;
using kCura.IntegrationPoints.Data.Factories;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Statistics.Implementations
{
    class ErrorFilesSizeStatistics : IErrorFilesSizeStatistics
    {
        private readonly IHelper _helper;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IAPILog _logger;
        private readonly IDbContextFactory _dbContextFactory;

        private const string _FOR_JOBHISTORY_ERROR = "Failed to retrieve total files size for job history id: {JobHistoryArtifactId}.";

        public ErrorFilesSizeStatistics(IHelper helper, IRepositoryFactory repositoryFactory)
        {
            _helper = helper;
            _repositoryFactory = repositoryFactory;
            _logger = _helper.GetLoggerFactory().GetLogger().ForContext<ErrorFilesSizeStatistics>();
            _dbContextFactory = new DbContextFactory(_helper);
        }

        public long ForJobHistoryOmmitedFiles(int workspaceArtifactId, int jobHistoryArtifactId)
        {
            try
            {
                int documentIdentifierArtifactId = _repositoryFactory
                    .GetFieldQueryRepository(workspaceArtifactId)
                    .RetrieveIdentifierField((int)ArtifactType.Document)
                    .ArtifactId;

                string documentIdentifierColumn = _repositoryFactory
                    .GetFieldRepository(workspaceArtifactId)
                    .Read(documentIdentifierArtifactId)
                    .ColumnName;

                string sqlText =
                    $@"    SELECT COALESCE(SUM([Size]),0)
                    FROM    [ExtendedArtifact] EA
                            JOIN [JobHistoryError] JHE ON EA.ArtifactID = JHE.ArtifactID
                            JOIN [Document] D ON JHE.SourceUniqueID = D.{documentIdentifierColumn}
                            JOIN [File] F ON D.ArtifactID = F.DocumentArtifactID
                    WHERE    [ParentArtifactID] = @artifactId";

                IEnumerable<SqlParameter> sqlParams = new[]
                {
                    new SqlParameter("@artifactId", SqlDbType.Int)
                    {
                        Value = jobHistoryArtifactId
                    }
                };

                IWorkspaceDBContext dbContext = _dbContextFactory.CreateWorkspaceDbContext(workspaceArtifactId);
                return dbContext.ExecuteSqlStatementAsScalar<long>(sqlText, sqlParams);
            }
            catch (Exception e)
            {
                _logger.LogError(e, _FOR_JOBHISTORY_ERROR, jobHistoryArtifactId);
                return 0L;
            }
        }
    }
}
