using System;
using System.Data;
using System.Linq;
using kCura.WinEDDS;
using kCura.WinEDDS.Api;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

using RAPI = Relativity;

namespace kCura.IntegrationPoints.ImportProvider.Parser
{
	public class DataReaderFactory : IDataReaderFactory
	{
		private IWinEddsLoadFileFactory _winEddsLoadFileFactory;
		private IWinEddsFileReaderFactory _winEddsFileReaderFactory;
		private IFieldParserFactory _fieldParserFactory;

		public DataReaderFactory(IFieldParserFactory fieldParserFactory,
			IWinEddsLoadFileFactory winEddsLoadFileFactory,
			IWinEddsFileReaderFactory winEddsFileReaderFactory)
		{
			_fieldParserFactory = fieldParserFactory;
			_winEddsLoadFileFactory = winEddsLoadFileFactory;
			_winEddsFileReaderFactory = winEddsFileReaderFactory;
		}

		public IDataReader GetDataReader(FieldMap[] fieldMaps, string options)
		{
			//TODO: use injected Iserializer instead
			ImportProviderSettings providerSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<ImportProviderSettings>(options);
			if (int.Parse(providerSettings.ImportType) == (int)ImportType.ImportTypeValue.Document)
			{
				LoadFileDataReader lfdr = GetLoadFileDataReader(fieldMaps, providerSettings);
				ImportDataReader idr = new ImportDataReader(lfdr);
				idr.Setup(fieldMaps);
				return idr;
			}
			else
			{
				return GetOpticonDataReader(providerSettings);
			}
		}

		private OpticonDataReader GetOpticonDataReader(ImportProviderSettings settings)
		{
			ImageLoadFile config = _winEddsLoadFileFactory.GetImageLoadFile(settings);
			IImageReader reader = _winEddsFileReaderFactory.GetOpticonFileReader(config);
			OpticonDataReader rv = new OpticonDataReader(settings, config, reader);
			rv.Init();
			return rv;
		}

		private LoadFileDataReader GetLoadFileDataReader(FieldMap[] fieldMaps, ImportProviderSettings settings)
		{
			string fieldIdentifierColumnName = fieldMaps.FirstOrDefault(x => x.SourceField.IsIdentifier)?.SourceField.DisplayName;
			
			LoadFile loadFile = _winEddsLoadFileFactory.GetLoadFile(settings);

			//Add columns to the LoadFile object
			IFieldParser fieldParser = _fieldParserFactory.GetFieldParser(settings);
			int colIdx = 0;
			foreach (string col in fieldParser.GetFields())
			{
				int fieldCat = (! string.IsNullOrEmpty(fieldIdentifierColumnName) && col == fieldIdentifierColumnName)
					? (int)RAPI.FieldCategory.Identifier
					: -1;

				DocumentField newDocField = new DocumentField(col, colIdx, 4, fieldCat, -1, -1, -1, false,
					kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice.LeaveBlankValuesUnchanged, false);

				LoadFileFieldMap.LoadFileFieldMapItem mapItem = new LoadFileFieldMap.LoadFileFieldMapItem(newDocField, colIdx++);
				loadFile.FieldMap.Add(mapItem);
			}

			IArtifactReader reader = _winEddsFileReaderFactory.GetLoadFileReader(loadFile);

			LoadFileDataReader rv = new LoadFileDataReader(settings, loadFile, reader);
			rv.Init();
			return rv;
		}
	}
}
