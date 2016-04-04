using Microsoft.VisualBasic;
using System;

namespace kCura.Windows.Process
{
	public class ErrorFileReader : kCura.Utility.DelimitedFileImporter
	{

		public override object ReadFile(string path)
		{
			this.Reader = new System.IO.StreamReader(path);
			System.Data.DataTable retval = new System.Data.DataTable();
			retval.Columns.Add("Key");
			retval.Columns.Add("Status");
			retval.Columns.Add("Description");
			retval.Columns.Add("Timestamp");
			Int32 i = 0;
			while (!this.HasReachedEOF && i < 1000) {
				retval.Rows.Add(this.GetLine());
				i += 1;
			}
			this.Close();
			object[] os = {
				retval,
				i >= 1000
			};
			return os;
		}

		public ErrorFileReader(bool doRetryLogic) : base( ",", "", Strings.ChrW(20).ToString(), doRetryLogic)
		{
		}
	}
}
