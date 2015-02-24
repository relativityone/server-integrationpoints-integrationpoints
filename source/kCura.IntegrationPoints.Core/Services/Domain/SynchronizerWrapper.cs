using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Syncronizer;
using kCura.IntegrationPoints.Core.Contracts;

namespace kCura.IntegrationPoints.Core.Domain
{
	public class SynchronizerWrapper : MarshalByRefObject, IDataSyncronizer
	{
		private readonly IDataSyncronizer _syncronizer;
		public SynchronizerWrapper(IDataSyncronizer syncronizer)
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
