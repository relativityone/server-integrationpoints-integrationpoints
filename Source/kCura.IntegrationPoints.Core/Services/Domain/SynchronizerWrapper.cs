using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;

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
#pragma warning disable 612
				_synchronizer.SyncData(data, fieldMap, options);
#pragma warning restore 612
			}
			catch (Exception e)
			{
				throw Utils.GetNonCustomException(e);
			}
		}

	    public void SyncData(IDataTransferContext context, IEnumerable<FieldMap> fieldMap, string options)
	    {
            try
            {
                _synchronizer.SyncData(context, fieldMap, options);
            }
            catch (Exception e)
            {
                throw Utils.GetNonCustomException(e);
            }
        }
	}
}
