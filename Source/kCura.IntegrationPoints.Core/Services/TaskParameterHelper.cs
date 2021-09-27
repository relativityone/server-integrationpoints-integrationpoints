using System;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.IntegrationPoints.Core.Services
{
	public class TaskParameterHelper : ITaskParameterHelper
	{
		private readonly Apps.Common.Utils.Serializers.ISerializer _serializer;
		private readonly IGuidService _guidService;
		public TaskParameterHelper(Apps.Common.Utils.Serializers.ISerializer serializer, IGuidService guidService)
		{
			_serializer = serializer;
			_guidService = guidService;
		}

		public virtual Guid GetBatchInstance(Job job)
		{
			Guid newBatchInstance = _guidService.NewGuid();
			if (!string.IsNullOrWhiteSpace(job.JobDetails))
			{
				try
				{
					newBatchInstance = _serializer.Deserialize<TaskParameters>(job.JobDetails).BatchInstance;
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to get Batch Instance.", ex);
				}
			}
			return newBatchInstance;
		}


	}
}
