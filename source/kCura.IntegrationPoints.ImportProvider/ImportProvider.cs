using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider
{
	[kCura.IntegrationPoints.Contracts.DataSourceProvider(Constants.Guids.ImportProviderEventHandler)]
	public class ImportProvider : kCura.IntegrationPoints.Contracts.Provider.IDataSourceProvider
	{
		private IFieldParserFactory _fieldParserFactory;
		private IDataReaderFactory _dataReaderFactory;
		private IEnumerableParserFactory _enumerableParserFactory;

		public ImportProvider(IFieldParserFactory fieldParserFactory, IDataReaderFactory dataReaderFactory, IEnumerableParserFactory enumerableParserFactory)
		{
			_fieldParserFactory = fieldParserFactory;
			_dataReaderFactory = dataReaderFactory;
			_enumerableParserFactory = enumerableParserFactory;
		}

		public IEnumerable<FieldEntry> GetFields(string options)
		{
			ImportProviderSettings settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportProviderSettings>(options);
			IFieldParser parser = _fieldParserFactory.GetFieldParser(settings);
			List<FieldEntry> result = new List<FieldEntry>();
			int idx = 0;
			foreach (string fieldName in parser.GetFields())
			{
				result.Add(new FieldEntry
				{
					DisplayName = fieldName,
					FieldIdentifier = idx.ToString(),
					FieldType = FieldType.String,
					IsIdentifier = idx == 0
				});
				idx++;
			}
			return result;
		}

		public IDataReader GetBatchableIds(FieldEntry identifier, string options)
		{
			ImportProviderSettings settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportProviderSettings>(options);
			return _dataReaderFactory.GetDataReader(settings);
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> sourceFileLines, string options)
		{
			ImportProviderSettings settings = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportProviderSettings>(options);
			if (int.Parse(settings.ImportType) == (int)ImportType.ImportTypeValue.Document)
			{
				return GetDataForDocumentRdo(settings, fields, sourceFileLines);
			}
			else
			{
				return GetDataForImage(settings, sourceFileLines);
			}
		}

		private IDataReader GetDataForDocumentRdo(ImportProviderSettings settings, IEnumerable<FieldEntry> fields, IEnumerable<string> sourceFileLines)
		{
			string loadFileDir = Path.GetDirectoryName(settings.LoadFile);
			//Get extracted text & native fields for relative path modifications
			bool extractedTextHasPathInfo = !string.IsNullOrEmpty(settings.ExtractedTextPathFieldIdentifier);
			bool nativeFileHasPathInfo = !string.IsNullOrEmpty(settings.NativeFilePathFieldIdentifier);

			IEnumerable<string[]> enumerableParser = _enumerableParserFactory.GetEnumerableParser(sourceFileLines, settings);

			DataTable dt = new DataTable();
			foreach (FieldEntry field in fields)
			{
				dt.Columns.Add(field.FieldIdentifier);
			}

			foreach (string[] sourceRow in enumerableParser)
			{
				DataRow dtRow = dt.NewRow();
				foreach (FieldEntry field in fields)
				{
					string colValue = sourceRow[Int32.Parse(field.FieldIdentifier)];
					if (((extractedTextHasPathInfo && field.FieldIdentifier == settings.ExtractedTextPathFieldIdentifier)
						|| (nativeFileHasPathInfo && field.FieldIdentifier == settings.NativeFilePathFieldIdentifier))
						&& !Path.IsPathRooted(colValue)) //Do not rewrite paths if column contains full path info
					{
						dtRow[field.FieldIdentifier] = Path.Combine(loadFileDir, colValue);
					}
					else
					{
						dtRow[field.FieldIdentifier] = colValue;
					}
				}
				dt.Rows.Add(dtRow);
			}

			return dt.CreateDataReader();
		}

		private IDataReader GetDataForImage(ImportProviderSettings settings, IEnumerable<string> sourceFileLines)
		{
			string loadFileDir = Path.GetDirectoryName(settings.LoadFile);
			IEnumerable<string[]> enumerableParser = _enumerableParserFactory.GetEnumerableParser(sourceFileLines, settings);
			DataTable dt = new DataTable();
			dt.Columns.Add(OpticonInfo.BATES_NUMBER_FIELD_NAME);
			dt.Columns.Add(OpticonInfo.FILE_LOCATION_FIELD_NAME);
			dt.Columns.Add(OpticonInfo.DOCUMENT_ID_FIELD_NAME);

			foreach (string[] sourceRow in enumerableParser)
			{
				DataRow dtRow = dt.NewRow();
				dtRow[OpticonInfo.BATES_NUMBER_FIELD_NAME] = sourceRow[OpticonInfo.BATES_NUMBER_FIELD_INDEX];
				string fileLocationColumnValue = sourceRow[OpticonInfo.FILE_LOCATION_FIELD_INDEX];
				//Account for relative paths in the load file
				if (!Path.IsPathRooted(fileLocationColumnValue)) {
					dtRow[OpticonInfo.FILE_LOCATION_FIELD_NAME] = Path.Combine(loadFileDir, fileLocationColumnValue);
				}
				else
				{
					dtRow[OpticonInfo.FILE_LOCATION_FIELD_NAME] = fileLocationColumnValue;
				}
				dtRow[OpticonInfo.DOCUMENT_ID_FIELD_NAME] = sourceRow[OpticonInfo.DOCUMENT_ID_FIELD_INDEX];
				dt.Rows.Add(dtRow);
			}

			return dt.CreateDataReader();
		}
	}
}
