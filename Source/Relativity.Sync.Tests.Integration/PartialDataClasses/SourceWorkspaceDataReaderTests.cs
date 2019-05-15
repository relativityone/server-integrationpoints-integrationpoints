using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Integration
{
	internal sealed partial class SourceWorkspaceDataReaderTests
	{
		private static Func<string, object>[] _converters =
		{
			x => x,
			x => Int32.Parse(x, CultureInfo.InvariantCulture),
			x => x,
			x => x,
			x => Int32.Parse(x, CultureInfo.InvariantCulture),
			x => Int32.Parse(x, CultureInfo.InvariantCulture),
			x => x
		};

		private static List<Dictionary<string, object>> GenerateMultipleBatchesTestCase(int workspaceArtifactId, int jobArtifactId)
		{
			const string template = @"
Control Number|NativeFileSize|NativeFileFilename|NativeFileLocation|SourceWorkspace|SourceJob|FolderPath
TST0001|100|foo.txt|\\test\foo\foo.txt|{0}|{1}|
TST0002|101|bat.txt|\\test\foo\bat.txt|{0}|{1}|
TST0003|102|bar.txt|\\test\foo\bar.txt|{0}|{1}|
TST0004|103|ban.txt|\\test\foo\ban.txt|{0}|{1}|
TST0005|104|baz.txt|\\test\boo\baz.txt|{0}|{1}|
";
			string data = string.Format(CultureInfo.InvariantCulture, template, workspaceArtifactId, jobArtifactId);
			return FromTestData(data);
		}

		private static List<Dictionary<string, object>> FromTestData(string data)
		{
			string[] lines = data.Split('\r', '\n');
			string[] columnNames = lines[0].Split('|');
			List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
			for (int i = 1; i < lines.Length; i++)
			{
				string line = lines[i].Trim();
				if (!string.IsNullOrEmpty(line))
				{
					string[] rowData = line.Split('|');
					if (_converters.Length != rowData.Length)
					{
						throw new ArgumentException($"Invalid number of rows (expected {_converters.Length}, got {rowData.Length}");
					}

					Dictionary<string, object> row = new Dictionary<string, object>();
					for (int j = 0; j < rowData.Length; j++)
					{
						object datum = _converters[j](rowData[j]);
						row.Add(columnNames[j], datum);
					}

					rows.Add(row);
				}
			}

			return rows;
		}
	}
}
