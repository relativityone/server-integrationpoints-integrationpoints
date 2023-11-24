﻿using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace kCura.IntegrationPoints.Core.Services.Conversion
{
    public class DataReaderToEnumerableService
    {
        private readonly IObjectBuilder _objectBuilder;

        public DataReaderToEnumerableService(IObjectBuilder objectBuilder)
        {
            _objectBuilder = objectBuilder;
        }

        public IEnumerable<T> GetData<T>(IDataReader reader)
        {
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
        }
    }
}
