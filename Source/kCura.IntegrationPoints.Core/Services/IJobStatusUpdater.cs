using System;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IJobStatusUpdater
	{
		Choice GenerateStatus(Guid batchId);
		Choice GenerateStatus(Data.JobHistory jobHistory);
	}
}
