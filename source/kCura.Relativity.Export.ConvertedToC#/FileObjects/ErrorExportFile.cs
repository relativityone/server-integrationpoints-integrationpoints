using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.FileObjects
{

	public class ErrorExportFile : ExportFile
	{
		private string _errorMessage = string.Empty;
		public ErrorExportFile(string errorMessage) : base(-1)
		{
			if (string.IsNullOrEmpty(errorMessage))
				throw new System.ArgumentException("Error message cannot be null for an error export file");
			_errorMessage = errorMessage;
		}
		public string ErrorMessage {
			get { return _errorMessage; }
		}
	}
}
