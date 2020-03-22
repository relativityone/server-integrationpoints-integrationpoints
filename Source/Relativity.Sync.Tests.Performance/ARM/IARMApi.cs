using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Sync.Tests.Performance.ARM.Contracts;

namespace Relativity.Sync.Tests.Performance.ARM
{
	[Headers("Content-Type: application/json", "X-CSRF-Header: -")]
	public interface IARMApi
	{
		[Post("/Relativity.ARM/Configuration/SetConfigurationData")]
		Task SetConfigurationAsync([Body] ContractEnvelope<ArmConfiguration> configuration);

		[Post("/Relativity.ARM/Jobs/Restore/Create")]
		Task<Job> CreateRestoreJobAsync([Body] ContractEnvelope<RestoreJob> restoreJob);

		[Post("/Relativity.ARM/Jobs/Action/Run")]
		Task RunJobAsync([Body] Job job);

		[Post("/Relativity.ARM/Jobs/Status/ReadJob")]
		Task<JobStatus> GetJobStatus(Job job);
	}
}
