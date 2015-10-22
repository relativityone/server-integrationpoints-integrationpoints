using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Domain
{
	public class SynchronizerWrapper : MarshalByRefObject, IDataSynchronizer
	{
		private readonly IDataSynchronizer _syncronizer;
		public SynchronizerWrapper(IDataSynchronizer syncronizer)
		{
			_syncronizer = syncronizer;
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			try
			{
				return _syncronizer.GetFields(options).ToList();
			}
			catch (Exception e)
			{
				throw Utils.GetNonCustomException(e);
			}
		}

		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
		{
			try
			{
				_syncronizer.SyncData(data, fieldMap, options);
			}
			catch (Exception e)
			{
				throw Utils.GetNonCustomException(e);
			}
		}
	}
}
