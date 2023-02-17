using kCura.IntegrationPoints.Core.Contracts.Import;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;
using System;

namespace kCura.IntegrationPoints.Core.Contracts.Agent
{
    public class TaskParametersBuilder : ITaskParametersBuilder
    {
        private readonly IImportFileLocationService _importFileLocationService;

        public TaskParametersBuilder(IImportFileLocationService importFileLocationService)
        {
            _importFileLocationService = importFileLocationService;
        }

        public TaskParameters Build(TaskType taskType, Guid batchInstanceId, string sourceConfiguration, string destinationConfiguration)
        {
            TaskParameters parameters = new TaskParameters
            {
                BatchInstance = batchInstanceId
            };

            switch (taskType)
            {
                case TaskType.ImportService:
                    parameters.BatchParameters = BuildLoadFileParameters(sourceConfiguration, destinationConfiguration);
                    break;
            }

            return parameters;
        }

        private LoadFileTaskParameters BuildLoadFileParameters(string sourceConfiguration, string destinationConfiguration)
        {
            LoadFileInfo loadFile = _importFileLocationService.LoadFileInfo(sourceConfiguration, destinationConfiguration);

            return new LoadFileTaskParameters
            {
                Size = loadFile.Size,
                LastModifiedDate = loadFile.LastModifiedDate,
                ProcessedItemsCount = 0
            };
        }
    }
}
