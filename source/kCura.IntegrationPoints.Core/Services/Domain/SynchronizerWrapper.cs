using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Core.Domain
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
			try
			{
				return _synchronizer.GetFields(options).ToList();
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
				_synchronizer.SyncData(data, fieldMap, options);
			}
			catch (Exception e)
			{
				throw Utils.GetNonCustomException(e);
			}
		}

		public void SyncData(IDataReader data, IEnumerable<FieldMap> fieldMap, string options)
		{
			try
			{
				_synchronizer.SyncData(data, fieldMap, options);
			}
			catch (Exception e)
			{
				throw Utils.GetNonCustomException(e);
			}
		}
	}
}
