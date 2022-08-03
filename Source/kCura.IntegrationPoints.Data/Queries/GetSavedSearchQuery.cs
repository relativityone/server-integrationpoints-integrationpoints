using Relativity.API;
using Relativity.Services;
using Relativity.Services.Search;

namespace kCura.IntegrationPoints.Data.Queries
{
    public class GetSavedSearchQuery
    {
        private readonly IServicesMgr _servicesMgr;
        private readonly int _workspaceArtifactId;
        private readonly int _savedSearchArtifactId;

        public GetSavedSearchQuery(IServicesMgr servicesMgr, int workspaceArtifactId, int savedSearchArtifactId)
        {
            _servicesMgr = servicesMgr;
            _workspaceArtifactId = workspaceArtifactId;
            _savedSearchArtifactId = savedSearchArtifactId;
        }

        public KeywordSearchQueryResultSet ExecuteQuery()
        {
            using (IKeywordSearchManager keywordSearchManager = _servicesMgr.CreateProxy<IKeywordSearchManager>(ExecutionIdentity.CurrentUser))
            {
                KeywordSearchQueryResultSet queryResult = keywordSearchManager.QueryAsync(_workspaceArtifactId, new Query()
                {
                    Condition = $"'Artifact ID' == {_savedSearchArtifactId}"
                }).GetAwaiter().GetResult();

                return queryResult;
            }
        }
    }
}
