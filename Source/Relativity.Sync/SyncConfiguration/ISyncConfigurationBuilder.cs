using Relativity.Sync.SyncConfiguration.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.SyncConfiguration
{
	/// <summary>
	/// Main Sync configuration builder class.
	/// </summary>
	public interface ISyncConfigurationBuilder
	{
		/// <summary>
		/// Configures Sync RDOs.
		/// </summary>
		/// <param name="rdoOptions">Sync RDO options.</param>
		ISyncJobConfigurationBuilder ConfigureRdos(RdoOptions rdoOptions);
	}
}
