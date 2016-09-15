using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.ImportProvider
{
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
            throw new NotImplementedException();
        }
    }
}
