using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Exports
{
	public class TempTextFileDeletor
	{
		private System.Collections.Generic.IEnumerable<string> _filepaths;
		public TempTextFileDeletor(System.Collections.Generic.IEnumerable<string> filePaths)
		{
			_filepaths = filePaths;
			if (_filepaths == null)
				_filepaths = new List<string>();
		}
		public void DeleteFiles()
		{
			foreach (string path in _filepaths) {
				if (!string.IsNullOrEmpty(path))
					kCura.Utility.File.Instance.Delete(path);
			}
		}
	}
}
