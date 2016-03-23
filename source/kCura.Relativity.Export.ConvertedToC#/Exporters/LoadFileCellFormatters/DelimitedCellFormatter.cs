using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using kCura.Relativity.Export.Exports;
using kCura.Relativity.Export.FileObjects;

namespace kCura.Relativity.Export.Exports
{
	public class DelimitedCellFormatter : ILoadFileCellFormatter
	{

		private ExportFile _settings;
		public DelimitedCellFormatter(ExportFile settings)
		{
			_settings = settings;
		}
		public string TransformToCell(string contents)
		{
			contents = contents.Replace(System.Environment.NewLine, Strings.ChrW(10).ToString());
			contents = contents.Replace(Strings.ChrW(13), Strings.ChrW(10));
			contents = contents.Replace(Strings.ChrW(10), _settings.NewlineDelimiter);
		    var text = _settings.QuoteDelimiter.ToString() + _settings.QuoteDelimiter;
            contents = contents.Replace(_settings.QuoteDelimiter.ToString(), text);
			return string.Format("{0}{1}{0}", _settings.QuoteDelimiter, contents);
		}

		public string RowPrefix {
			get { return string.Empty; }
		}

		public string RowSuffix {
			get { return string.Empty; }
		}

		public string CreateImageCell(ObjectExportInfo artifact)
		{
			return string.Empty;
		}

		public string CreateNativeCell(string location, ObjectExportInfo artifact)
		{
			return string.Format("{2}{0}{1}{0}", _settings.QuoteDelimiter, location, _settings.RecordDelimiter);
		}
	}
}
