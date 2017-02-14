using System;
using kCura.IntegrationPoints.Domain.Synchronizer;

namespace kCura.IntegrationPoints.Domain
{
	//internal for now, make public when we switch to allowing this side to be plugable.
	internal class DefaultSynchronizerFactory : ISynchronizerFactory
	{
		public IDataSynchronizer CreateSynchronizer(Guid identifier, string options, string credentials)
		{
			throw new NotImplementedException();
		}

		public IDataSynchronizer CreateSynchronizer(Guid identifier, string options)
		{
			throw new NotImplementedException();
		}
	}
}
