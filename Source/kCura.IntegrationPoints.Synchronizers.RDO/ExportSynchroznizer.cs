using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class ExportSynchroznizer : RdoFieldSynchronizerBase, IDataSynchronizer
    {
	    private IFileQuery _fileQuery;
        public ExportSynchroznizer(IRelativityFieldQuery fieldQuery, IImportApiFactory factory, IFileQuery fileQuery, IHelper helper) : base(fieldQuery, factory, helper)
        {
            _fileQuery = fileQuery;
        }

	    public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
	    {
	    }

	    public void SyncData(IDataReader data, IEnumerable<FieldMap> fieldMap, string options)
	    {
	    }
    }
}