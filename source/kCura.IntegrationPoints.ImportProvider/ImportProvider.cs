using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.ImportProvider.Helpers.Logging;

namespace kCura.IntegrationPoints.ImportProvider
{
    [kCura.IntegrationPoints.Contracts.DataSourceProvider(Constants.Guids.ImportProviderEventHandler)]
    public class ImportProvider : kCura.IntegrationPoints.Contracts.Provider.IDataSourceProvider
    {
        public IDataReader GetBatchableIds(FieldEntry identifier, string options)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FieldEntry> GetFields(string options)
        {
            SeqLogger.Info("Got Options: {Options}", options);

            var result = new List<FieldEntry>();
            result.Add(new FieldEntry
            {
                DisplayName = "JustThisOneField",
                FieldIdentifier = "JustThisOneField",
                FieldType = FieldType.String,
                IsIdentifier = true
            });

            SeqLogger.Info("Finished constructing result. About to return...");

            return result;
        }
    }
}
