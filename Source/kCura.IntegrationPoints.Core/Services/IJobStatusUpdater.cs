using System;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IJobStatusUpdater
	{
		Choice GenerateStatus(Guid batchId, long workspaceId);
		Choice GenerateStatus(Data.JobHistory jobHistory, long workspaceId);
	}
}
