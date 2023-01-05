using System.Collections.Generic;
using System.Data;
using System.Text;
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
            StringBuilder exceptionMessageBuilder = new StringBuilder();
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
                    exceptionMessageBuilder.AppendLine(ex.Message);
                    continue;
                }
                yield return _objectBuilder.BuildObject<T>(reader);
            }

            string exceptionMessage = exceptionMessageBuilder.ToString();
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                throw new MalformedLineException(exceptionMessage);
            }

            _diagnosticLog.LogDiagnostic("Data read finished.");
        }
    }
}
