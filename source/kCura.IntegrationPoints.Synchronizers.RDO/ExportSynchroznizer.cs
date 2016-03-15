using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Synchronizer;
using kCura.IntegrationPoints.Data.Queries;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class ExportSynchroznizer : RdoFieldSynchronizerBase, IDataSynchronizer
    {
	    private IFileQuery _fileQuery;
        public ExportSynchroznizer(IRelativityFieldQuery fieldQuery, IImportApiFactory factory, IFileQuery fileQuery) : base(fieldQuery, factory)
        {
            _fileQuery = fileQuery;
        }

	    public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
	    {
	    }

	    public void SyncData(IEnumerable<string> entryIds, IDataReader data, IEnumerable<FieldMap> fieldMap, string options)
		{
            ImportSettings settings = JsonConvert.DeserializeObject<ImportSettings>(options);

            var nativeFilePaths = _fileQuery.GetDocumentFiles(string.Join(",", entryIds), 0);

            foreach (var path in nativeFilePaths)
            {
                File.Copy(path.Location, settings.Fileshare + "\\" + path.Filename);
            }
        }
    }
}