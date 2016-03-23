using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Types
{
	public class LoadFileType
	{
		public enum FileFormat
		{
			Opticon = 0,
			IPRO = 1,
			IPRO_FullText = 2
		}

		public static System.Data.DataTable GetLoadFileTypes()
		{
			System.Data.DataTable dt = new System.Data.DataTable();
			dt.Columns.Add("DisplayName");
			dt.Columns.Add("Value", typeof(Int32));
			dt.Rows.Add(new object[] {
				"Select...",
				-1
			});
			dt.Rows.Add(new object[] {
				"Opticon",
				0
			});
			dt.Rows.Add(new object[] {
				"IPRO",
				1
			});
			dt.Rows.Add(new object[] {
				"IPRO (FullText)",
				2
			});
			return dt;
		}
	}
}
