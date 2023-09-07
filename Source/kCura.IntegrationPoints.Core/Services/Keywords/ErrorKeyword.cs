using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
    public class ErrorKeyword : IKeyword
    {
        public string KeywordName { get { return "\\[ERROR]"; } }

        private readonly Job _job;
        private readonly JobHistoryErrorQuery _query;
        private readonly IJobHistoryService _historyService;
        private readonly ISerializer _serializer;

        public ErrorKeyword(Job job, JobHistoryErrorQuery query, IJobHistoryService historyService, ISerializer serializer)
        {
            _job = job;
            _query = query;
            _historyService = historyService;
            _serializer = serializer;
        }

        public string Convert()
        {
            var history = _historyService.GetRdoWithoutDocuments(Guid.Parse(_job.CorrelationID));
            var error = _query.GetJobLevelError(history.ArtifactId);
            return error.Error;
        }
    }
}
