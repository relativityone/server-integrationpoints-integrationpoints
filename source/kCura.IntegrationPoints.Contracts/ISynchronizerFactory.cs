using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Syncronizer;

namespace kCura.IntegrationPoints.Contracts
{
	/// <summary>
	/// Provides a means to create a Synchronizer in the outside app domain.
	/// </summary>
	public interface ISynchronizerFactory
	{
		/// <summary>
		/// Creates a new syncronizer based on the identifier and the options.
		/// </summary>
		/// <param name="identifier">A guid representing the identifier to find the DataSynchronizer.</param>
		/// <param name="options">The options specific to the current integration point identifier.</param>
		/// <returns>A new instance of the data synchronizer.</returns>
		IDataSyncronizer CreateSyncronizer(Guid identifier, string options);
	}
}
