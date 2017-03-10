using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider
{
	[Contracts.DataSourceProvider(Constants.Guids.ImportProviderEventHandler)]
	public class ImportProvider : Contracts.Provider.IDataSourceProvider
	{
		IFieldParserFactory _fieldParserFactory;

		public ImportProvider(IFieldParserFactory fieldParserFactory)
		{
			_fieldParserFactory = fieldParserFactory;
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
			throw new NotImplementedException("GetBatchableIds should not be called on ImportProvider");
		}

		public IDataReader GetData(IEnumerable<FieldEntry> fields, IEnumerable<string> sourceFileLines, string options)
		{
			throw new NotImplementedException("GetData should not be called on ImportProvider");
		}
	}
}
