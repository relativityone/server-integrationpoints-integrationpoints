using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Domain.Logging;

namespace kCura.IntegrationPoints.Core.Services.Conversion
{
    public class DataReaderToEnumerableService
    {
        private readonly IObjectBuilder _objectBuilder;
        private readonly IDiagnosticLog _diagnosticLog;

        public DataReaderToEnumerableService(IObjectBuilder objectBuilder, IDiagnosticLog diagnosticLog)
        {
            _objectBuilder = objectBuilder;
            _diagnosticLog = diagnosticLog;
        }

        public IEnumerable<T> GetData<T>(IDataReader reader)
        {
            _diagnosticLog.LogDiagnostic("Start reading data from DataReader.");
            while (reader.Read())
            {
                yield return _objectBuilder.BuildObject<T>(reader);
            }

            _diagnosticLog.LogDiagnostic("Data read finished.");
        }
    }
}
