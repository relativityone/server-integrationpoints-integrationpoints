using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Domain.Logging;
using Microsoft.VisualBasic.FileIO;

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
            bool readSuccessfully = true;
            MalformedLineException malformedLineException = null;
            while (readSuccessfully)
            {
                try
                {
                    readSuccessfully = reader.Read();
                    if (!readSuccessfully)
                    {
                        break;
                    }
                }
                catch (MalformedLineException ex)
                {
                    string message = malformedLineException == null ? ex.Message : $"{malformedLineException.Message}\n{ex.Message}";
                    malformedLineException = new MalformedLineException(message);
                    continue;
                }
                yield return _objectBuilder.BuildObject<T>(reader);
            }

            if (malformedLineException != null)
            {
                throw malformedLineException;
            }

            _diagnosticLog.LogDiagnostic("Data read finished.");
        }
    }
}
