using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;

namespace kCura.IntegrationPoints.Core.Services.Domain
{
	public class SynchronizerWrapper : MarshalByRefObject, IDataSynchronizer
	{
		private readonly IDataSynchronizer _synchronizer;
		public SynchronizerWrapper(IDataSynchronizer synchronizer)
		{
			_synchronizer = synchronizer;
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
				return _synchronizer.GetFields(options).ToList();
		}

		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
		{
				_synchronizer.SyncData(data, fieldMap, options);
		}

	    public void SyncData(IDataTransferContext context, IEnumerable<FieldMap> fieldMap, string options)
	    {
                _synchronizer.SyncData(context, fieldMap, options);
        }
	}
}
