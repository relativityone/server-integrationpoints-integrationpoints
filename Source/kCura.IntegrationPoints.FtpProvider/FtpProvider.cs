using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.FtpProvider.Parser;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Renci.SshNet.Common;
using Constants = kCura.IntegrationPoints.FtpProvider.Helpers.Constants;

namespace kCura.IntegrationPoints.FtpProvider
{
    [DataSourceProvider(Constants.Guids.FtpProviderEventHandler)]
    public class FtpProvider : IDataSourceProvider
    {
        private readonly IConnectorFactory _connectorFactory;
        private readonly IDataReaderFactory _dataReaderFactory;
        private readonly IAPILog _logger;
        private readonly IParserFactory _parserFactory;
        private readonly ISettingsManager _settingsManager;

        public FtpProvider(IConnectorFactory connectorFactory, ISettingsManager settingsManager, IParserFactory parserFactory, IDataReaderFactory dataReaderFactory,
            IHelper helper)
        {
            _connectorFactory = connectorFactory;
            _settingsManager = settingsManager;
            _parserFactory = parserFactory;
            _dataReaderFactory = dataReaderFactory;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<FtpProvider>();
        }

        public IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
        {
            LogRetrievingFields();
            List<FieldEntry> fields = new List<FieldEntry>();
            string fileName = string.Empty;
            string remoteLocation = string.Empty;
            
            Settings settings = GetSettingsModel(providerConfiguration.Configuration);
            try
            {
                SecuredConfiguration securedConfiguration = _settingsManager.DeserializeCredentials(providerConfiguration.SecuredConfiguration);

                string csvInput = AddFileExtension(settings.Filename_Prefix);
                fileName = GetDynamicFileName(Path.GetFileName(csvInput), settings.Timezone_Offset);
                remoteLocation = Path.GetDirectoryName(FormatPath(csvInput));
                using (var client = _connectorFactory.GetConnector(settings.Protocol, settings.Host, settings.Port, securedConfiguration.Username, securedConfiguration.Password))
                {
                    using (Stream stream = client.DownloadStream(remoteLocation, fileName, Constants.RetyCount))
                    {
                        using (var parser = _parserFactory.GetDelimitedFileParser(stream, ParserOptions.GetDefaultParserOptions()))
                        {
                            IEnumerable<string> columns = parser.ParseColumns();
                            fields.AddRange(columns.Select(name => new FieldEntry() { DisplayName = name, FieldIdentifier = name }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is SftpPathNotFoundException || (ex is WebException && ex.ToString().Contains("(550) File unavailable")))
                {
                    var message = $"Unable to access file on remote location. Exception: {ex}";
                    LogRetrievingFieldsErrorWithDetails(ex, message);
                    throw new Exception(message);
                }
                else if ((ex is WebException && (ex.ToString().Contains("The underlying connection was closed")) ||
                        ex.ToString().Contains("The remote server returned an error")))
                {
                    //TODO: There is a problem with disposing FtpConnector object that needs to be further investigated and fixed.
                    //This is to hide the issue because data is corretly parsed
                    LogRetrievingFieldsWarning(ex, ex.Message);
                }
                else
                {
                    LogRetrievingFieldsError(ex);
                    throw;
                }
            }

            return fields;
        }

        public IDataReader GetBatchableIds(FieldEntry identifier, DataSourceProviderConfiguration providerConfiguration)
        {
            LogRetrievingBatchableIds(identifier);
            IDataReader retVal;
            string fileName = string.Empty;
            string remoteLocation = string.Empty;
            Settings settings = GetSettingsModel(providerConfiguration.Configuration);
            ParserOptions parserOptions = ParserOptions.GetDefaultParserOptions();
            try
            {
                SecuredConfiguration securedConfiguration = _settingsManager.DeserializeCredentials(providerConfiguration.SecuredConfiguration);

                string csvInput = AddFileExtension(settings.Filename_Prefix);
                fileName = GetDynamicFileName(Path.GetFileName(csvInput), settings.Timezone_Offset);
                remoteLocation = Path.GetDirectoryName(FormatPath(csvInput));
                using (IFtpConnector client = _connectorFactory.GetConnector(settings.Protocol, settings.Host, settings.Port, securedConfiguration.Username, securedConfiguration.Password))
                {
                    string fileLocation = Path.GetTempPath() + Guid.NewGuid() + ".csv";
                    client.DownloadFile(fileLocation, remoteLocation, fileName, Constants.RetyCount);
                    IDataReader fileReader = _dataReaderFactory.GetFileDataReader(fileLocation);
                    if (parserOptions.FirstLineContainsColumnNames)
                    {
                        //since column list and order is recorded at the last Integration Point save,
                        //verify that current file has the same structure
                        string columns = fileReader.GetString(0);
                        ValidateColumns(columns, settings, parserOptions);
                    }
                    retVal = fileReader;
                    return retVal;
                }
            }
            catch (Exception ex)
            {
                if (ex is SftpPathNotFoundException || (ex is WebException && ex.ToString().Contains("(550) File unavailable")))
                {
                    var message = $"Unable to access: {remoteLocation}{fileName} {ex}";
                    LogRetrievingBatchableIdsErrorWithDetails(identifier, ex, message);
                    throw new Exception(message);
                }
                
                LogRetrievingBatchableIdsError(identifier, ex);
                throw new IntegrationPointsException(
                    "Failed to extract batch IDs when downloading file from FTP/SFTP server", ex)
                {
                    ShouldAddToErrorsTab = true
                };
            }
        }

        public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, DataSourceProviderConfiguration providerConfiguration)
        {
            List<string> entryIdsList = entryIds.ToList();
            LogRetrievingData(entryIdsList);
            Settings settings = GetSettingsModel(providerConfiguration.Configuration);
            ParserOptions parserOptions = ParserOptions.GetDefaultParserOptions();
            parserOptions.FirstLineContainsColumnNames = false;
            try
            {
                List<string> columnList = settings.ColumnList.Select(x => x.FieldIdentifier).ToList();
                TextReader reader = _dataReaderFactory.GetEnumerableReader(entryIdsList);
                IParser parser = _parserFactory.GetDelimitedFileParser(reader, parserOptions, columnList);
                IDataReader dataReader = parser.ParseData();
                return dataReader;
            }
            catch (Exception ex)
            {
                if (ex is SftpPathNotFoundException || (ex is WebException && ex.ToString().Contains("(550) File unavailable")))
                {
                    var message = $"Unable to access: {ex}";
                    LogRetrievingDataErrorWithDetails(entryIdsList, ex, message);
                    throw new Exception(message);
                }
                else if ((ex is WebException && (ex.ToString().Contains("The underlying connection was closed")) ||
                        ex.ToString().Contains("The remote server returned an error")))
                {
                    //TODO: There is a problem with disposing FtpConnector object that needs to be further investigated and fixed.
                    //This is to hide the issue because data is corretly parsed
                    LogRetrievingDataWarning(ex, ex.Message);
                }
                else
                {
                    LogRetrievingDataError(entryIdsList, ex);
                    throw;
                }
            }

            return null;
        }

        internal DateTime GetCurrentTime(int? offset)
        {
            var ts = new TimeSpan(0, 0, -offset.GetValueOrDefault(0), 0);
            return DateTime.UtcNow.Add(ts);
        }

        internal string FormatPath(string path)
        {
            return FilenameFormatter.FormatFtpPath(path);
        }

        internal string GetDynamicFileName(string input, int? offset)
        {
            var retVal = input.Trim();
            retVal = FilenameFormatter.FormatFilename(retVal, Constants.WildCard, GetCurrentTime(offset));
            return retVal;
        }

        internal string AddFileExtension(string input)
        {
            var retVal = input.Trim();
            if ((retVal.Length < 4) || (String.CompareOrdinal(retVal.Substring(retVal.Length - 4).ToLowerInvariant(), ".csv") != 0))
            {
                retVal = retVal + ".csv";
            }
            return retVal;
        }

        public Settings GetSettingsModel(string options)
        {
            return _settingsManager.DeserializeSettings(options);
        }

        internal void ValidateColumns(string columns, Settings settings, ParserOptions parserOptions)
        {
            //must contain the same columns in the same order as it was initially when Integration Point was saved
            string expectedColumns = string.Join(parserOptions.Delimiters[0], settings.ColumnList.Select(x => x.FieldIdentifier).ToList());

            string fixedColumns = string.Join(",", columns.Split(',').Select(item => item.Trim(' ', '"')));

            if (!expectedColumns.Equals(fixedColumns, StringComparison.InvariantCultureIgnoreCase))
            {
                LogValidatingColumnsError();
                throw new Helpers.Exceptions.ColumnsMissmatchException();
            }
        }

        #region Logging

        private void LogRetrievingFields()
        {
            _logger.LogInformation("Attempting to get fields in FTP Provider.");
        }

        private void LogRetrievingBatchableIds(FieldEntry identifier)
        {
            _logger.LogInformation("Attempting to get batchable ids in FTP Provider for field {FieldIdentifier}.", identifier.FieldIdentifier);
        }

        private void LogRetrievingData(IList<string> entryIds)
        {
            _logger.LogInformation("Attempting to get data in FTP Provider for IDs count {count}", entryIds.Count);
        }

        private void LogRetrievingFieldsError(Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve fields in FTP Provider.");
        }

        private void LogRetrievingFieldsErrorWithDetails(Exception ex, string message)
        {
            _logger.LogError(ex, "Failed to retrieve fields in FTP Provider. Details: {Message}.", message);
        }

        private void LogRetrievingFieldsWarning(Exception ex, string message)
        {
            _logger.LogWarning(ex, "Error occured while retrieving fields. Details: {Message}", message);
        }

        private void LogRetrievingBatchableIdsError(FieldEntry identifier, Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve batchable ids in FTP Provider for field {FieldIdentifier}.", identifier.FieldIdentifier);
        }

        private void LogRetrievingBatchableIdsErrorWithDetails(FieldEntry identifier, Exception ex, string message)
        {
            _logger.LogError(ex,
                "Failed to retrieve batchable ids in FTP Provider for field {FieldIdentifier}. Details: {Message}.",
                identifier.FieldIdentifier, message);
        }
        
        private void LogRetrievingDataError(IList<string> entryIds, Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve data in FTP Provider for IDs count {count}.", entryIds.Count);
        }

        private void LogRetrievingDataWarning(Exception ex, string message)
        {
            _logger.LogWarning(ex, "Error occured while retrieving data. Details: {Message}", message);
        }

        private void LogRetrievingDataErrorWithDetails(IList<string> entryIds, Exception ex, string message)
        {
            _logger.LogError(ex, "Failed to retrieve data in FTP Provider for IDs count {count}. Details: {Message}.",
                entryIds.Count, message);
        }

        private void LogValidatingColumnsError()
        {
            _logger.LogError("Invalid columns in input file.");
        }

        #endregion
    }
}