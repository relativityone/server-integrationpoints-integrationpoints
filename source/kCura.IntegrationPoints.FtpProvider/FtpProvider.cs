using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

        public FtpProvider(IConnectorFactory connectorFactory, ISettingsManager settingsManager, IParserFactory parserFactory)
        {
            _connectorFactory = connectorFactory;
            _settingsManager = settingsManager;
            _parserFactory = parserFactory;
        }

        public IEnumerable<FieldEntry> GetFields(string options)
        {
            var retVal = new List<FieldEntry>();
            var fileName = String.Empty;
            var remoteLocation = String.Empty;
            var modelOptions = GetSettingsModel(options);
            try
            {
                var csvInput = AddFileExtension(modelOptions.Filename_Prefix);
                fileName = GetDynamicFileName(Path.GetFileName(csvInput), modelOptions.Timezone_Offset);
                remoteLocation = Path.GetDirectoryName(FormatPath(csvInput));
                using (var client = GetClient(modelOptions))
                {
                    using (Stream stream = client.DownloadStream(remoteLocation, fileName, Constants.RetyCount))
                    {
                        using (var parser = _parserFactory.GetDelimitedFileParser(stream, Constants.Delimiter))
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
        //public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
        //{
        //    //AK: distribution POC
        //    IDataReader retVal;
        //    var fileName = String.Empty;
        //    var remoteLocation = String.Empty;
        //    var modelOptions = GetSettingsModel(options);
        //    try
        //    {
        //        var parser = new DelimitedFileParser2(entryIds, Constants.Delimiter);
        //        retVal = parser.ParseData();

        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is SftpPathNotFoundException || (ex is System.Net.WebException && ex.ToString().Contains("(550) File unavailable")))
        //        {
        //            throw new Exception("Unable to access: " + remoteLocation + fileName + " " + ex.ToString());
        //        }
        //        else
        //        {
        //            throw new Exception(ex.ToString());
        //        }
        //    }

        //    return retVal;
        //}

        //public IDataReader GetBatchableIds(FieldEntry identifier, string options)
        //{
        //    //AK: distribution POC
        //    IDataReader retVal;
        //    var fileName = String.Empty;
        //    var remoteLocation = String.Empty;
        //    var modelOptions = GetSettingsModel(options);
        //    try
        //    {
        //        var csvInput = AddFileExtension(modelOptions.Filename_Prefix);
        //        fileName = GetDynamicFileName(Path.GetFileName(csvInput), modelOptions.Timezone_Offset);
        //        remoteLocation = Path.GetDirectoryName(FormatPath(csvInput));
        //        using (var client = GetClient(modelOptions))
        //        {
        //            var fileLocation = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
        //            client.DownloadFile(fileLocation, remoteLocation, fileName, Constants.RetyCount);
        //            var reader = new CsvDataReader(fileLocation);
        //            retVal = reader;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is SftpPathNotFoundException || (ex is System.Net.WebException && ex.ToString().Contains("(550) File unavailable")))
        //        {
        //            throw new Exception("Unable to access: " + remoteLocation + fileName + " " + ex.ToString());
        //        }
        //        else
        //        {
        //            throw new Exception(ex.ToString());
        //        }
        //    }

        //    return retVal;
        //}

        public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
        {
            IDataReader retVal;
            var fileName = String.Empty;
            var remoteLocation = String.Empty;
            var modelOptions = GetSettingsModel(options);
            try
            {
                var csvInput = AddFileExtension(modelOptions.Filename_Prefix);
                fileName = GetDynamicFileName(Path.GetFileName(csvInput), modelOptions.Timezone_Offset);
                remoteLocation = Path.GetDirectoryName(FormatPath(csvInput));
                using (var client = GetClient(modelOptions))
                {
                    var fileLocation = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";
                    client.DownloadFile(fileLocation, remoteLocation, fileName, Constants.RetyCount);
                    var parser = new DelimitedFileParser(fileLocation, Constants.Delimiter);
                    retVal = parser.ParseData();
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

        public IDataReader GetBatchableIds(FieldEntry identifier, string options)
        {
            //we don't need custom batching because RIP automatically creates batches of 1000
            var dt = new DataTable();
            dt.Columns.Add("ID");
            var row = dt.NewRow();
            row["ID"] = 1;
            dt.Rows.Add(row);
            return dt.CreateDataReader();
        }

        internal DateTime GetCurrentTime(Int32? offset)
        {
            var ts = new TimeSpan(0, 0, -offset.GetValueOrDefault(0), 0);
            return DateTime.UtcNow.Add(ts);
        }

        internal IFtpConnector GetClient(Settings settings)
        {
            IFtpConnector client;
            if (settings.Protocol == ProtocolName.FTP)
            {
                client = _connectorFactory.CreateFtpConnector(settings.Host, settings.Port.GetValueOrDefault(21), settings.Username, settings.Password);
            }
            else
            {
                client = _connectorFactory.CreateSftpConnector(settings.Host, settings.Port.GetValueOrDefault(22), settings.Username, settings.Password);
            }
            return client;
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
    }
}
