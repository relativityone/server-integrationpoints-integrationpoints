using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            bool readSuccessfuly = true;
            MalformedLineException malformedLineException = new MalformedLineException();
            while (readSuccessfuly)
            {
                try
                {
                    readSuccessfuly = reader.Read();
                    if (!readSuccessfuly)
                    {
                        break;
                    }
                }
                catch (MalformedLineException ex)
                {
                    malformedLineException = new MalformedLineException($"{malformedLineException.Message} {ex.Message}");
                    continue;
                }
                yield return _objectBuilder.BuildObject<T>(reader);
            }

            if (!string.IsNullOrEmpty(malformedLineException.Message))
            {
                throw malformedLineException;
            }

            _diagnosticLog.LogDiagnostic("Data read finished.");
        }
    }
}
