using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using kCura.Relativity.Export.Exports;

namespace kCura.Relativity.Export.Exports
{
	public interface ILoadFileCellFormatter
	{
		string TransformToCell(string contents);
		string CreateNativeCell(string location, ObjectExportInfo artifact);
		string CreateImageCell(ObjectExportInfo artifact);

		string RowPrefix { get; }
		string RowSuffix { get; }
	}






}

