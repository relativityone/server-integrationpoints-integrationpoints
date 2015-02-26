using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoints.Core.Services
{
	interface IJobStatusUpdater
	{
		Choice GenerateStatus(JobHistory jobHistory);
		//{
		//is this this the last job in the queue for this JobHistory
		//
		// if there is at least one Job level error - Job error
		//	else if at least one item level error - completed with errors
		//	else Completed
		//
		//}
	}
}
