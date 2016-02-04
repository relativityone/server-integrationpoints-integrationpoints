using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Synchronizer;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// Provides a method used to create a synchronizer in an external application domain.
	/// </summary>
	public interface ISynchronizerFactory
	{
		/// <summary>
		/// Creates a new synchonizer based on an identifier and the options.
		/// </summary>
		/// <param name="identifier">A GUID identifing the synchronizer.</param>
		/// <param name="options">The options specific to the current integration point identifier.</param>
		/// <returns>A new instance of the data synchronizer.</returns>
		IDataSynchronizer CreateSynchronizer(Guid identifier, string options);
	}
}
