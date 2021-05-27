using kCura.IntegrationPoints.Core.Contracts.Import;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;
using System;
using FileInfo = System.IO.FileInfo;

namespace kCura.IntegrationPoints.Core.Contracts.Agent
{
	public class TaskParametersBuilder : ITaskParametersBuilder
	{
		private readonly IImportFileLocationService _importFileLocationService;

		public TaskParametersBuilder(IImportFileLocationService importFileLocationService)
		{
			_importFileLocationService = importFileLocationService;
		}

		public TaskParameters Build(TaskType taskType, Guid batchInstanceId, IntegrationPoint integrationPoint)
		{
			TaskParameters parameters = new TaskParameters
			{
				BatchInstance = batchInstanceId
			};

			switch(taskType)
			{
				case TaskType.ImportService:
					parameters.BatchParameters = BuildLoadFileParameters(integrationPoint);
					break;
				default:
					break;
			}

			return parameters;
		}

		private LoadFileTaskParameters BuildLoadFileParameters(IntegrationPoint integrationPoint)
		{
			LoadFileInfo loadFile = _importFileLocationService.LoadFileInfo(integrationPoint);

			return new LoadFileTaskParameters
			{
				Size = loadFile.Size,
				LastModifiedDate = loadFile.LastModifiedDate
			};
		}
	}
}
