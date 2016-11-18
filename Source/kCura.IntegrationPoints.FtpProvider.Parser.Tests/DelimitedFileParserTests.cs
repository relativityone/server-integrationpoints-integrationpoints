using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FtpProvider.Parser.Tests
{
	[TestFixture]
	public class DelimitedFileParserTests : TestBase
	{
		[SetUp]
		public override void SetUp()
		{

		}

		[Test, System.ComponentModel.Description("Validates that when the fileLocation constructor is used, the solution should not use the filestream ")]
		public void FileStreamNullWhenLocationConstructorUsed()
		{
			var location = CreateFile("Sample Text for temp file");
			using (var parser = new DelimitedFileParser(location, new ParserOptions() { Delimiters = new[] { "," } }))
			{
				Assert.IsTrue(parser._fileStream == null);
			}
			DeleteFile(location);
		}

		[Test, System.ComponentModel.Description("Validates that when the stream constructor is used, the solution should not use the filelocation ")]
		public void FileLocationNullWhenStreamConstructorUsed()
		{
			var streamInput = StringToStream("Test String");
			var parser = new DelimitedFileParser(streamInput, new ParserOptions() { Delimiters = new[] { "," } });
			Assert.IsTrue(parser._fileLocation == null);
		}

		[Test, System.ComponentModel.Description("Validates logic that determines when a file exists")]
		public void SourceExistsValidationDetectsfile()
		{
			var location = CreateFile("Sample Text for temp file");
			using (var parser = new DelimitedFileParser(location, new ParserOptions() { Delimiters = new[] { "," } }))
			{
				Assert.DoesNotThrow(() => parser.SourceExists());
			}
			DeleteFile(location);
		}

		[Test, System.ComponentModel.Description("Validates logic that determines when a file does not exists")]
		public void SourceExistsValidationDetectsMissingfile()
		{
			var location = GenerateTempLocation();

			Assert.Throws<Exceptions.CantAccessSourceException>(
				() => new DelimitedFileParser(location, new ParserOptions() { Delimiters = new[] { "," } }));
		}

		[Test, System.ComponentModel.Description("Validates that columns are parsed correctly")]
		public void ValidateColumnParsing()
		{
			var input = @"""Column1"", ""Column2"", ""Column3""";
			var streamInput = StringToStream(input);
			var parser = new DelimitedFileParser(streamInput, new ParserOptions() { Delimiters = new[] { "," } });
			var result = parser.ParseColumns();
			Assert.AreEqual(result.Count(), 3);
		}

		[Test, System.ComponentModel.Description("Validate that blank columns are not allowed")]
		public void ValidateBlankColumnsNotAllowed()
		{
			var input = "Hello, this is a test!";
			var streamInput = StringToStream(input);
			var parser = new DelimitedFileParser(streamInput, new ParserOptions() { Delimiters = new[] { "," } });
			String[] columns = { "Column1", "", "COlumn2" };

			Assert.Throws<Exceptions.BlankColumnExcepetion>(() => parser.ValidateColumns(columns));
		}

		[Test, System.ComponentModel.Description("Validate that duplicate columns are not allowed")]
		public void ValidateDuplicateColumnsNotAllowed()
		{
			var input = "Hello, this is a test!";
			var streamInput = StringToStream(input);
			var parser = new DelimitedFileParser(streamInput, new ParserOptions() { Delimiters = new[] { "," } });
			String[] columns = { "Column1", "Column1", "Column2" };

			Assert.Throws<Exceptions.DuplicateColumnsExistExcepetion>(() => parser.ValidateColumns(columns));
		}

		[Test, System.ComponentModel.Description("Validate columns are added to the datareader")]
		public void ValidateColumnsAreAddedToDataReader()
		{
			/*Creates csv formatted text for input:
            Column1,Column2,Column3
            Data1,Data2,Data3
            */
			String[] columns = { "Column1", "Column2", "Column3" };
			String[] data = { "Data1", "Data2", "Data3" };
			var delimiter = ",";
			var input = String.Join(delimiter, columns) + Environment.NewLine + String.Join(delimiter, data);

			var streamInput = StringToStream(input);
			var parser = new DelimitedFileParser(streamInput, new ParserOptions() { FirstLineContainsColumnNames = true, Delimiters = new[] { "," } });
			var parsedData = parser.ParseData();

			var parsedColumns = GetDataReaderColumns(parsedData);
			Assert.AreEqual(columns.Length, parsedColumns.Count());
			foreach (var column in columns)
			{
				Assert.IsTrue(parsedColumns.Contains(column));
			}
		}

		[Test, System.ComponentModel.Description("Validate that all data is added to the datareader")]
		public void ValidateAllDataIsAddedToDataReader()
		{
			/*Creates csv formatted text for input:
            Column1,Column2,Column3
            Data1,Data2,Data3
            */
			String[] columns = { "Column1", "Column2", "Column3" };
			String[] data = { "Data1", "Data2", "Data3" };
			var delimiter = ",";
			var input = String.Join(delimiter, columns) + Environment.NewLine + String.Join(delimiter, data);

			var streamInput = StringToStream(input);
			var parser = new DelimitedFileParser(streamInput, new ParserOptions() { FirstLineContainsColumnNames = true, Delimiters = new[] { "," } });
			var parsedData = parser.ParseData();

			var isClosed = parsedData.IsClosed;
			var fieldCount = parsedData.FieldCount;

			Object[] row = new Object[parsedData.FieldCount];
			Assert.AreEqual(data.Length, row.Length);
			while (parsedData.Read())
			{
				parsedData.GetValues(row);
				foreach (var item in data)
				{
					Assert.IsTrue(row.Contains(item));
				}
			}

		}

		[Test, System.ComponentModel.Description("Validate that all data is added to the datareader")]
		public void ValidateAllDataIsAddedToDataReader2()
		{
			/*Creates csv formatted text for input:
            Column1,Column2,Column3
            Data1,Data2,Data3
            */
			String[] columns = { "Column1", "Column2", "Column3" };
			String[] data = { "Data1", "Data2", "Data3" };
			var delimiter = ",";
			var input = String.Join(delimiter, columns) + Environment.NewLine + String.Join(delimiter, data);

			var streamInput = StringToStream(input);
			var parser = new DelimitedFileParser(streamInput, new ParserOptions() { FirstLineContainsColumnNames = true, Delimiters = new[] { "," } });
			var parsedData = parser.ParseData();

			var isClosed = parsedData.IsClosed;
			var fieldCount = parsedData.FieldCount;

			Object[] row = new Object[parsedData.FieldCount];
			Assert.AreEqual(data.Length, row.Length);
			while (parsedData.Read())
			{
				parsedData.GetValues(row);
				foreach (var item in data)
				{
					Assert.IsTrue(row.Contains(item));
				}
			}

		}

		private IEnumerable<String> GetDataReaderColumns(IDataReader rdr)
		{
			var retVal = new List<string>();
			for (var i = 0; i < rdr.FieldCount; i++)
			{
				retVal.Add(rdr.GetName(i));
			}
			return retVal;
		}

		private Stream StringToStream(String input)
		{
			var bytes = Encoding.UTF8.GetBytes(input);
			return new MemoryStream(bytes);
		}

		private String CreateFile(String contents)
		{
			var fileLocation = GenerateTempLocation();
			using (FileStream target = File.Create(fileLocation))
			{
				using (StreamWriter writer = new StreamWriter(target))
				{
					writer.WriteLine(contents);
				}
			}
			return fileLocation;
		}

		private void DeleteFile(String path)
		{
			File.Delete(path);
		}

		private String GenerateTempLocation()
		{
			return (System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv");
		}
	}
}
