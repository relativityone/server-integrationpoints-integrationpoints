using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Synchronizer;

namespace kCura.IntegrationPoints.Contracts
{
	//internal for now, make public when we switch to allowing this side to be plugable.
	internal class DefaultSynchronizerFactory : ISynchronizerFactory
	{
		public IDataSynchronizer CreateSynchronizer(Guid identifier, string options)
		{
			throw new NotImplementedException();
		}
	}
}
