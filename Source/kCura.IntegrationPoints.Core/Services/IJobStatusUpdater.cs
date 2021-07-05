using System;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IJobStatusUpdater
	{
		ChoiceRef GenerateStatus(Guid batchId);
		ChoiceRef GenerateStatus(Data.JobHistory jobHistory, long? jobId = null);
	}
}
