﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.FtpProvider.Parser;
using kCura.IntegrationPoints.FtpProvider.Parser.Interfaces;
using Relativity.API;
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

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			LogRetrievingFields();
			List<FieldEntry> retVal = new List<FieldEntry>();
			string fileName = string.Empty;
			string remoteLocation = string.Empty;
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
				if (ex is SftpPathNotFoundException || (ex is WebException && ex.ToString().Contains("(550) File unavailable")))
				{
					var message = $"Unable to access: {remoteLocation}{fileName} {ex}";
					LogRetrievingFieldsErrorWithDetails(ex, message);
					throw new Exception(message);
				}
				else if ((ex is WebException && (ex.ToString().Contains("The underlying connection was closed")) ||
						ex.ToString().Contains("The remote server returned an error")))
				{
					//TODO: There is a problem with disposing FtpConnector object that needs to be further investigated and fixed.
					//This is to hide the issue because data is corretly parsed
				}
				else
				{
					LogRetrievingFieldsError(ex);
					throw new Exception(ex.ToString());
				}
			}

			return retVal;
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			LogRetrievingBatchableIds(identifier);
			IDataReader retVal;
			string fileName = string.Empty;
			string remoteLocation = string.Empty;
			Settings settings = GetSettingsModel(options);
			ParserOptions parserOptions = ParserOptions.GetDefaultParserOptions();
			try
			{
				string csvInput = AddFileExtension(settings.Filename_Prefix);
				fileName = GetDynamicFileName(Path.GetFileName(csvInput), settings.Timezone_Offset);
				remoteLocation = Path.GetDirectoryName(FormatPath(csvInput));
				using (IFtpConnector client = _connectorFactory.GetConnector(settings.Protocol, settings.Host, settings.Port, settings.Username, settings.Password))
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
				else if ((ex is WebException && (ex.ToString().Contains("The underlying connection was closed")) ||
						ex.ToString().Contains("The remote server returned an error")))
				{
					//TODO: There is a problem with disposing FtpConnector object that needs to be further investigated and fixed.
					//This is to hide the issue because data is corretly parsed
				}
				else
				{
					LogRetrievingBatchableIdsError(identifier, ex);
					throw new Exception(ex.ToString());
				}
			}

			return null;
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> entryIds, string options)
		{
			LogRetrievingData(entryIds);
			IDataReader retVal;
			string fileName = string.Empty;
			string remoteLocation = string.Empty;
			Settings settings = GetSettingsModel(options);
			ParserOptions parserOptions = ParserOptions.GetDefaultParserOptions();
			parserOptions.FirstLineContainsColumnNames = false;
			try
			{
				List<string> columnList = settings.ColumnList.Select(x => x.FieldIdentifier).ToList();
				TextReader reader = _dataReaderFactory.GetEnumerableReader(entryIds);
				IParser parser = _parserFactory.GetDelimitedFileParser(reader, parserOptions, columnList);
				retVal = parser.ParseData();

				return retVal;
			}
			catch (Exception ex)
			{
				if (ex is SftpPathNotFoundException || (ex is WebException && ex.ToString().Contains("(550) File unavailable")))
				{
					var message = $"Unable to access: {remoteLocation}{fileName} {ex}";
					LogRetrievingDataErrorWithDetails(entryIds, ex, message);
					throw new Exception(message);
				}
				else if ((ex is WebException && (ex.ToString().Contains("The underlying connection was closed")) ||
						ex.ToString().Contains("The remote server returned an error")))
				{
					//TODO: There is a problem with disposing FtpConnector object that needs to be further investigated and fixed.
					//This is to hide the issue because data is corretly parsed
				}
				else
				{
					LogRetrievingDataError(entryIds, ex);
					throw new Exception(ex.ToString());
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
			if ((retVal.Length < 4) || (string.Compare(retVal.Substring(retVal.Length - 4).ToLower(), ".csv") != 0))
			{
				retVal = retVal + ".csv";
			}
			return retVal;
		}

		public Settings GetSettingsModel(string options)
		{
			return _settingsManager.ConvertFromEncryptedString(options);
		}

		internal void ValidateColumns(string columns, Settings settings, ParserOptions parserOptions)
		{
			//must contain the same columns in the same order as it was initially when Integration Point was saved
			string expectedColumns = string.Join(parserOptions.Delimiters[0], settings.ColumnList.Select(x => x.FieldIdentifier).ToList());

			string fixedColumns = string.Join(",", columns.Split(',').Select(item => item.Trim().TrimStart('"').TrimEnd('"')));

			if (!expectedColumns.Equals(fixedColumns, StringComparison.InvariantCultureIgnoreCase))
			{
				LogValidatingColumnsError(columns, expectedColumns);
				LogValidatingColumnsError(columns, expectedColumns);
				throw new Exceptions.ColumnsMissmatchExcepetion();
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

		private void LogRetrievingData(IEnumerable<string> entryIds)
		{
			_logger.LogInformation("Attempting to get data in FTP Provider for ids: {Ids}.", string.Join(",", entryIds));
		}

		private void LogRetrievingFieldsError(Exception ex)
		{
			_logger.LogError(ex, "Failed to retrieve fields in FTP Provider.");
		}

		private void LogRetrievingFieldsErrorWithDetails(Exception ex, string message)
		{
			_logger.LogError(ex, "Failed to retrieve fields in FTP Provider. Details: {Message}.");
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

		private void LogRetrievingDataError(IEnumerable<string> entryIds, Exception ex)
		{
			_logger.LogError(ex, "Failed to retrieve data in FTP Provider for ids {Ids}.", string.Join(",", entryIds));
		}

		private void LogRetrievingDataErrorWithDetails(IEnumerable<string> entryIds, Exception ex, string message)
		{
			_logger.LogError(ex, "Failed to retrieve data in FTP Provider for ids {Ids}. Details: {Message}.",
				string.Join(",", entryIds), message);
		}

		private void LogValidatingColumnsError(string columns, string expectedColumns)
		{
			_logger.LogError("Invalid columns in input file. Expected columns: {ExpectedColumns}. Actual columns: {Columns}.", expectedColumns, columns);
		}

		#endregion
	}
}