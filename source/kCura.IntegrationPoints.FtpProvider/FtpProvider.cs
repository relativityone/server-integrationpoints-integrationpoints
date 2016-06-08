using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.FtpProvider.Parser;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;
using Renci.SshNet.Common;

namespace kCura.IntegrationPoints.FtpProvider
{
    [kCura.IntegrationPoints.Contracts.DataSourceProvider(Constants.Guids.FtpProviderEventHandler)]
    public class FtpProvider : kCura.IntegrationPoints.Contracts.Provider.IDataSourceProvider
    {
        private IConnectorFactory _connectorFactory;
        private ISettingsManager _settingsManager;
        private IParserFactory _parserFactory;
        private IDataReaderFactory _dataReaderFactory;

        public FtpProvider(IConnectorFactory connectorFactory, ISettingsManager settingsManager, IParserFactory parserFactory, IDataReaderFactory dataReaderFactory)
        {
            _connectorFactory = connectorFactory;
            _settingsManager = settingsManager;
            _parserFactory = parserFactory;
            _dataReaderFactory = dataReaderFactory;
        }

        public IEnumerable<FieldEntry> GetFields(string options)
        {
            List<FieldEntry> retVal = new List<FieldEntry>();
            string fileName = String.Empty;
            string remoteLocation = String.Empty;
            Settings settings = GetSettingsModel(options);
            try
            {
                var csvInput = AddFileExtension(settings.Filename_Prefix);
                fileName = GetDynamicFileName(Path.GetFileName(csvInput), settings.Timezone_Offset);
                remoteLocation = Path.GetDirectoryName(FormatPath(csvInput));
                using (var client = _connectorFactory.GetConnector(settings.Protocol, settings.Host, settings.Port, settings.Username, settings.Password))
                {
                    using (Stream stream = client.DownloadStream(remoteLocation, fileName, Constants.RetyCount))
                    {
                        using (var parser = _parserFactory.GetDelimitedFileParser(stream, ParserOptions.GetDefaultParserOptions()))
                        {
                            var columns = parser.ParseColumns();
                            foreach (var column in columns)
                            {
                                retVal.Add(new FieldEntry { DisplayName = column, FieldIdentifier = column });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is SftpPathNotFoundException || (ex is System.Net.WebException && ex.ToString().Contains("(550) File unavailable")))
                {
                    retVal.Add(new FieldEntry { DisplayName = "Unable to access: " + remoteLocation + fileName + " " + ex.ToString() });
                }
                else
                {
                    retVal.Add(new FieldEntry { DisplayName = ex.ToString() });
                }
            }

            return retVal;
        }

        public IDataReader GetBatchableIds(FieldEntry identifier, string options)
        {
            IDataReader retVal;
            string fileName = String.Empty;
            string remoteLocation = String.Empty;
            Settings settings = GetSettingsModel(options);
            ParserOptions parserOptions = ParserOptions.GetDefaultParserOptions();
            try
            {
                var csvInput = AddFileExtension(settings.Filename_Prefix);
                fileName = GetDynamicFileName(Path.GetFileName(csvInput), settings.Timezone_Offset);
                remoteLocation = Path.GetDirectoryName(FormatPath(csvInput));
                using (var client = _connectorFactory.GetConnector(settings.Protocol, settings.Host, settings.Port, settings.Username, settings.Password))
                {
                    var fileLocation = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
                    client.DownloadFile(fileLocation, remoteLocation, fileName, Constants.RetyCount);
                    var fileReader = _dataReaderFactory.GetFileDataReader(fileLocation);
                    if (parserOptions.FirstLineContainsColumnNames)
                    {
                        //since column list and order is recorded at the last Integration Point save,
                        //verify that current file has the same structure
                        string columns = fileReader.GetString(0);
                        ValidateColumns(columns, settings, parserOptions);
                    }
                    retVal = fileReader;
                }
            }
            catch (Exception ex)
            {
                if (ex is SftpPathNotFoundException || (ex is System.Net.WebException && ex.ToString().Contains("(550) File unavailable")))
                {
                    throw new Exception("Unable to access: " + remoteLocation + fileName + " " + ex.ToString());
                }
                else
                {
                    throw new Exception(ex.ToString());
                }
            }

            return retVal;
        }

        public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
        {
            IDataReader retVal;
            string fileName = String.Empty;
            string remoteLocation = String.Empty;
            Settings settings = GetSettingsModel(options);
            ParserOptions parserOptions = ParserOptions.GetDefaultParserOptions();
            parserOptions.FirstLineContainsColumnNames = false;
            try
            {
                List<string> columnList = settings.ColumnList.Select(x => x.FieldIdentifier).ToList();
                TextReader reader = _dataReaderFactory.GetEnumerableReader(entryIds);
                IParser parser = _parserFactory.GetDelimitedFileParser(reader, parserOptions, columnList);
                retVal = parser.ParseData();
            }
            catch (Exception ex)
            {
                if (ex is SftpPathNotFoundException || (ex is System.Net.WebException && ex.ToString().Contains("(550) File unavailable")))
                {
                    throw new Exception("Unable to access: " + remoteLocation + fileName + " " + ex.ToString());
                }
                else
                {
                    throw new Exception(ex.ToString());
                }
            }

            return retVal;
        }

        internal DateTime GetCurrentTime(Int32? offset)
        {
            var ts = new TimeSpan(0, 0, -offset.GetValueOrDefault(0), 0);
            return DateTime.UtcNow.Add(ts);
        }

        internal String FormatPath(String path)
        {
            var retVal = path.Trim();
            retVal = retVal.Replace("\\", "/");
            if (retVal[0] != '/')
            {
                retVal = "/" + retVal;
            }
            return retVal;
        }

        internal String GetDynamicFileName(String input, Int32? offset)
        {
            var retVal = input.Trim();
            retVal = FilenameFormatter.FormatFilename(retVal, Constants.WildCard, GetCurrentTime(offset));
            return retVal;
        }

        internal String AddFileExtension(String input)
        {
            var retVal = input.Trim();
            if (retVal.Length < 4 || String.Compare(retVal.Substring(retVal.Length - 4).ToLower(), ".csv") != 0)
            {
                retVal = retVal + ".csv";
            }
            return retVal;
        }

        public Settings GetSettingsModel(String options)
        {
            return _settingsManager.ConvertFromEncryptedString(options);
        }

        internal void ValidateColumns(string columns, Settings settings, ParserOptions parserOptions)
        {
            //must contain the same columns in the same order as it was initially when Integration Point was saved
            string expectedColumns = string.Join(parserOptions.Delimiters[0], settings.ColumnList.Select(x => x.FieldIdentifier).ToList());

            if (!expectedColumns.Equals(columns, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exceptions.ColumnsMissmatchExcepetion();
            }
        }
    }
}
