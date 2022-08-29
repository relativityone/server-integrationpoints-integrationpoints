using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Logging;
using Relativity.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Core.Services.Conversion
{
    public class SynchronizerObjectBuilder : IObjectBuilder
    {
        private readonly Dictionary<string, FieldEntry> _fieldsDictionary;
        private readonly IDiagnosticLog _diagnosticLog;

        public SynchronizerObjectBuilder(IEnumerable<FieldEntry> fields, IDiagnosticLog diagnosticLog)
        {
            _fieldsDictionary = fields.ToDictionary(k => k.FieldIdentifier, v => v);
            _diagnosticLog = diagnosticLog;

            _diagnosticLog.LogDiagnostic("DataReader fields: {fields}", string.Join(", ", _fieldsDictionary.Keys));
        }

        public T BuildObject<T>(System.Data.IDataRecord row)
        {
            IDictionary<FieldEntry, object> returnValue = new Dictionary<FieldEntry, object>();

            for (int i = 0; i < row.FieldCount; i++)
            {
                string name = row.GetName(i);

                bool wasRead = false;
                if (_fieldsDictionary.TryGetValue(name, out FieldEntry field))
                {
                    returnValue.Add(field, row[i]);
                    wasRead = true;
                }

                _diagnosticLog.LogDiagnostic("Read Value for {name} - {wasRead}", name, wasRead);
            }

            return (T)returnValue;
        }
    }
}
