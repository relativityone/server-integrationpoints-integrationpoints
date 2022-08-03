using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FtpProvider.Parser.Tests
{
    [TestFixture, Category("Unit")]
    public class DelimitedFileParserTests : TestBase
    {
        [SetUp]
        public override void SetUp()
        {

        }

        [Test, System.ComponentModel.Description("Validates that when the fileLocation constructor is used, the solution should not use the filestream ")]
        public void FileStreamNullWhenLocationConstructorUsed()
        {
            string location = CreateFile("Sample Text for temp file");
            using (var parser = new DelimitedFileParser(location, new ParserOptions() { Delimiters = new[] { "," } }))
            {
                Assert.IsNull(parser.FileStream);
                Assert.IsNotNull(parser.FileLocation);
            }
            DeleteFile(location);
        }

        [Test]
        public void ShouldParseColumnsWhenLocationConstructorUsed()
        {
            const int numberOfColumns = 2; 
            string location = CreateFile("Column1,Column2");
            using (var parser = new DelimitedFileParser(location, new ParserOptions() { Delimiters = new[] { "," }, FirstLineContainsColumnNames = true}))
            {
                Assert.IsNull(parser.FileStream);
                Assert.IsNotNull(parser.FileLocation);
                Assert.AreEqual(numberOfColumns, parser.Columns.Length);
            }
            DeleteFile(location);
        }

        [Test, System.ComponentModel.Description("Validates that when the stream constructor is used, the solution should not use the filelocation ")]
        public void FileLocationNullWhenStreamConstructorUsed()
        {
            Stream streamInput = StringToStream("Test String");
            var parser = new DelimitedFileParser(streamInput, new ParserOptions() { Delimiters = new[] { "," } });

            Assert.IsNull(parser.FileLocation);
            Assert.IsNotNull(parser.FileStream);
        }

        [Test]
        public void FileLocationNullWhenTextReaderConstructorUsed()
        {
            //Arrange
            string location = CreateFile("Sample Text for temp file");
            var columns = new List<string>(){ "Column1", "Column2", "Column3" };

            using (TextReader textReaderInput = File.OpenText(location))
            {
                var parser = new DelimitedFileParser(textReaderInput, new ParserOptions() { Delimiters = new[] { "," } }, columns);

                Assert.IsNull(parser.FileLocation);
                Assert.IsNull(parser.FileStream);
                Assert.IsNotNull(parser.TextReader);
                Assert.IsNotNull(parser.Parser);
            }
            DeleteFile(location);
        }

        [Test, System.ComponentModel.Description("Validates logic that determines when a file exists")]
        public void SourceExistsValidationDetectsfile()
        {
            string location = CreateFile("Sample Text for temp file");
            using (var parser = new DelimitedFileParser(location, new ParserOptions() { Delimiters = new[] { "," } }))
            {
                Assert.DoesNotThrow(() => parser.SourceExists());
            }
            DeleteFile(location);
        }

        [Test, System.ComponentModel.Description("Validates logic that determines when a file does not exists")]
        public void SourceExistsValidationDetectsMissingfile()
        {
            string location = GenerateTempLocation();

            Assert.Throws<Helpers.Exceptions.CantAccessSourceException>(
                () => new DelimitedFileParser(location, new ParserOptions() { Delimiters = new[] { "," } }));
        }

        [Test, System.ComponentModel.Description("Validates that columns are parsed correctly")]
        public void ValidateColumnParsing()
        {
            const int numberOflinesInInputStream = 1;
            var input = @"""Column1"", ""Column2"", ""Column3""";
            Stream streamInput = StringToStream(input);
            using (var parser = new DelimitedFileParser(streamInput, new ParserOptions() {Delimiters = new[] {","}}))
            {
                IEnumerable<string> parsedColumns = parser.ParseColumns();

                Assert.AreEqual(numberOflinesInInputStream, parser.RecordsAffected);
                Assert.AreEqual(parsedColumns.Count(), 3);
            }
        }

        [Test]
        public void ShouldNotValidateColumnsBecaseColumnsAlreadyExist()
        {
            //Arrange
            const int numberOfParsedLines = 0;
            var input = "dummy input";
            Stream streamInput = StringToStream(input);
            using (var parser = new DelimitedFileParser(streamInput, new ParserOptions() {Delimiters = new[] {","}}))
            {
                string[] columns = {"Column1", "Column1", "Column2"};

                parser.Columns = columns;

                //Act
                IEnumerable<string> parsedColumns = parser.ParseColumns();

                //Assert
                Assert.AreEqual(columns, parsedColumns);
                Assert.AreEqual(numberOfParsedLines, parser.RecordsAffected);
            }
        }

        [Test]
        public void ShouldThrowNoColumnsException()
        {
            var invalidColumns = "    ";

            //Arrange
            Stream streamInput = StringToStream(invalidColumns);
            using (var parser = new DelimitedFileParser(streamInput, new ParserOptions() {Delimiters = new[] {","}}))
            {
                //Assert
                Assert.Throws<Helpers.Exceptions.NoColumnsException>(() => parser.ParseColumns());
            }
        }

        [Test]
        public void ShouldReturnValidDepth()
        {
            const int expectedDepth = 1;
            var invalidColumns = "    ";

            //Arrange
            Stream streamInput = StringToStream(invalidColumns);
            using (var parser = new DelimitedFileParser(streamInput, new ParserOptions() {Delimiters = new[] {","}}))
            {
                //Assert
                Assert.AreEqual(expectedDepth, parser.Depth);
            }
        }

        [Test, System.ComponentModel.Description("Validate that blank columns are not allowed")]
        public void ValidateBlankColumnsNotAllowed()
        {
            var input = "Hello, this is a test!";
            Stream streamInput = StringToStream(input);
            using (var parser = new DelimitedFileParser(streamInput, new ParserOptions() {Delimiters = new[] {","}}))
            {
                string[] columns = {"Column1", "", "COlumn2"};

                Assert.Throws<Helpers.Exceptions.BlankColumnException>(() => parser.ValidateColumns(columns));
            }
        }

        [Test, System.ComponentModel.Description("Validate that duplicate columns are not allowed")]
        public void ValidateDuplicateColumnsNotAllowed()
        {
            var input = "Hello, this is a test!";
            Stream streamInput = StringToStream(input);
            using (var parser = new DelimitedFileParser(streamInput, new ParserOptions() {Delimiters = new[] {","}}))
            {
                string[] columns = {"Column1", "Column1", "Column2"};

                Assert.Throws<Helpers.Exceptions.DuplicateColumnsExistException>(() => parser.ValidateColumns(columns));
            }
        }

        [Test, System.ComponentModel.Description("Validate columns are added to the datareader")]
        public void ValidateColumnsAreAddedToDataReader()
        {
            /*Creates csv formatted text for input:
            Column1,Column2,Column3
            Data1,Data2,Data3
            */
            string[] columns = { "Column1", "Column2", "Column3" };
            string[] data = { "Data1", "Data2", "Data3" };
            var delimiter = ",";
            string input = string.Join(delimiter, columns) + Environment.NewLine + string.Join(delimiter, data);

            Stream streamInput = StringToStream(input);
            var dataReader = new DelimitedFileParser(streamInput, new ParserOptions() { FirstLineContainsColumnNames = true, Delimiters = new[] { "," } });

            IEnumerable<string> dataReaderColumns = GetDataReaderColumns(dataReader);
            Assert.AreEqual(columns.Length, dataReaderColumns.Count());
            foreach (string column in columns)
            {
                Assert.IsTrue(dataReaderColumns.Contains(column));
            }
        }

        [Test, System.ComponentModel.Description("Validate that all data is added to the datareader")]
        public void ValidateAllDataIsAddedToDataReader()
        {
            /*Creates csv formatted text for input:
            Column1,Column2,Column3
            Data1,Data2,Data3
            */
            string[] columns = { "Column1", "Column2", "Column3" };
            string[] data = { "Data1", "Data2", "Data3" };
            var delimiter = ",";
            string input = string.Join(delimiter, columns) + Environment.NewLine + string.Join(delimiter, data);

            Stream streamInput = StringToStream(input);
            using (var dataReader = new DelimitedFileParser(streamInput,
                new ParserOptions() {FirstLineContainsColumnNames = true, Delimiters = new[] {","}}))
            {
                var row = new object[dataReader.FieldCount];
                Assert.AreEqual(data.Length, row.Length);
                while (dataReader.Read())
                {
                    dataReader.GetValues(row);
                    foreach (string item in data)
                    {
                        Assert.IsTrue(row.Contains(item));
                    }
                }
            }
        }

        [Test]
        public void ShouldThrowNumberOfColumnsNotEqualToNumberOfDataValuesException()
        {
            /*Creates csv formatted text for input:
            Column1,Column3
            Data1,Data2,Data3
            */
            string[] columns = {"Column1", "Column3"};
            string[] data = {"Data1", "Data2", "Data3"};
            var delimiter = ",";
            string input = string.Join(delimiter, columns) + Environment.NewLine + string.Join(delimiter, data);

            Stream streamInput = StringToStream(input);
            using (var dataReader = new DelimitedFileParser(streamInput,
                new ParserOptions() {FirstLineContainsColumnNames = true, Delimiters = new[] {","}}))
            {
                Assert.Throws<Helpers.Exceptions.NumberOfColumnsNotEqualToNumberOfDataValuesException>(() => dataReader.Read());
            }
        }

        [Test]
        public void ShouldReturnDataReader()
        {
            Stream streamInput = StringToStream("Test String");
            using (var parser = new DelimitedFileParser(streamInput, new ParserOptions() {Delimiters = new[] {","}}))
            {
                Assert.IsInstanceOf<IDataReader>(parser.ParseData());
            }
        }

        [Test]
        public void ShouldBeClosed()
        {
            Stream streamInput = StringToStream("Test String");
            var dataReader = new DelimitedFileParser(streamInput, new ParserOptions() { Delimiters = new[] { "," } });
            
            //Assert, Act & Assert
            Assert.IsTrue(dataReader.FileStream.CanRead);

            dataReader.Close();

            Assert.IsTrue(dataReader.IsClosed);
            Assert.IsFalse(dataReader.FileStream.CanRead);
        }

        [Test]
        public void ShouldReturnDataByStringIndexer()
        {
            const int columnNumber = 1;
            string[] columns = {"Column1", "Column2", "Column3"};
            string[] data = {"Data1", "Data2", "Data3"};
            var delimiter = ",";
            string input = string.Join(delimiter, columns) + Environment.NewLine + string.Join(delimiter, data);

            Stream streamInput = StringToStream(input);
            using (var dataReader = new DelimitedFileParser(streamInput,
                new ParserOptions() { FirstLineContainsColumnNames = true, Delimiters = new[] { "," } }))
            {
                dataReader.Read();
                Assert.AreEqual(data[columnNumber], dataReader[columns[columnNumber]]);
            }
        }

        [Test]
        public void ShouldReturnSchemaTable()
        {
            string[] columns = { "Column1", "Column2", "Column3" };
            string[] data = { "Data1", "Data2", "Data3" };
            var delimiter = ",";
            string input = string.Join(delimiter, columns) + Environment.NewLine + string.Join(delimiter, data);

            Stream streamInput = StringToStream(input);
            using (var dataReader = new DelimitedFileParser(streamInput,
                new ParserOptions() { FirstLineContainsColumnNames = true, Delimiters = new[] { "," } }))
            {
                var dataTable = dataReader.GetSchemaTable();

                Assert.AreEqual(columns.Length, dataTable?.Rows.Count);
                for (var i = 0; i < columns.Length; i++)
                {
                    Assert.AreEqual(columns[i],dataTable?.Rows[i][0]);
                }
            }
        }

        [Test]
        public void ShouldReturnDataByIntegerIndexer()
        {
            const int columnNumber = 1;
            string[] columns = { "Column1", "Column2", "Column3" };
            string[] data = { "Data1", "Data2", "Data3" };
            var delimiter = ",";
            string input = string.Join(delimiter, columns) + Environment.NewLine + string.Join(delimiter, data);

            Stream streamInput = StringToStream(input);
            using (var dataReader = new DelimitedFileParser(streamInput,
                new ParserOptions() { FirstLineContainsColumnNames = true, Delimiters = new[] { "," } }))
            {
                Assert.AreEqual(columns[columnNumber], dataReader[columnNumber]);
                dataReader.Read();
                Assert.AreEqual(data[columnNumber], dataReader[columnNumber]);
            }
        }

        [Test]
        public void ShouldReturnDataParsedToSpecificTypes()
        {
            string[] columns = { "Boolean", "Number", "Float", "Guid", "Char", "DateTime", "Empty" };
            string[] data = { "True", "5", "0.5", "bd21d781-6c82-43f0-9091-b97084569441", "A", "2015-01-02", "" };
            var delimiter = ",";
            string input = string.Join(delimiter, columns) + Environment.NewLine + string.Join(delimiter, data);

            Stream streamInput = StringToStream(input);
            using (var dataReader = new DelimitedFileParser(streamInput,
                new ParserOptions() { FirstLineContainsColumnNames = true, Delimiters = new[] { "," } }))
            {
                dataReader.Read();

                Assert.AreEqual(true, dataReader.GetBoolean(0));
                
                Assert.AreEqual(5, dataReader.GetByte(1));
                Assert.AreEqual(typeof(byte), dataReader.GetByte(1).GetType());

                Assert.AreEqual(5, dataReader.GetInt16(1));
                Assert.AreEqual(typeof(short), dataReader.GetInt16(1).GetType());

                Assert.AreEqual(5, dataReader.GetInt32(1));
                Assert.AreEqual(typeof(int), dataReader.GetInt32(1).GetType());

                Assert.AreEqual(5, dataReader.GetInt64(1));
                Assert.AreEqual(typeof(long), dataReader.GetInt64(1).GetType());

                Assert.AreEqual(0.5f, dataReader.GetFloat(2));
                Assert.AreEqual(typeof(float), dataReader.GetFloat(2).GetType());

                Assert.AreEqual(0.5, dataReader.GetDouble(2));
                Assert.AreEqual(typeof(double), dataReader.GetDouble(2).GetType());
                
                Assert.AreEqual(typeof(string), dataReader.GetFieldType(2));
                
                Assert.AreEqual(new Guid(data[3]), dataReader.GetGuid(3));
                
                Assert.AreEqual(5, dataReader.GetDecimal(1));
                Assert.AreEqual(typeof(decimal), dataReader.GetDecimal(1).GetType());

                Assert.AreEqual('A', dataReader.GetChar(4));
                Assert.AreEqual(typeof(char), dataReader.GetChar(4).GetType());

                Assert.AreEqual("A", dataReader.GetString(4));
                Assert.AreEqual(typeof(string), dataReader.GetString(4).GetType());

                Assert.AreEqual(new DateTime(2015, 1, 2), dataReader.GetDateTime(5));

                Assert.IsTrue(dataReader.IsDBNull(6));

                Assert.Throws<NotSupportedException>(() => dataReader.GetData(0));
                Assert.Throws<NotSupportedException>(() => dataReader.GetDataTypeName(0));
                Assert.Throws<NotSupportedException>(() => dataReader.GetChars(0, 0, new char[] { }, 0, 0));
                Assert.Throws<NotSupportedException>(() => dataReader.GetBytes(0, 0, new byte[] { }, 0, 0));
            }
        }


        


        #region helpers
        private IEnumerable<string> GetDataReaderColumns(IDataReader rdr)
        {
            var retVal = new List<string>();
            for (var i = 0; i < rdr.FieldCount; i++)
            {
                retVal.Add(rdr.GetName(i));
            }
            return retVal;
        }

        private Stream StringToStream(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return new MemoryStream(bytes);
        }

        private string CreateFile(string contents)
        {
            string fileLocation = GenerateTempLocation();
            using (FileStream target = File.Create(fileLocation))
            {
                using (var writer = new StreamWriter(target))
                {
                    writer.WriteLine(contents);
                }
            }
            return fileLocation;
        }

        private void DeleteFile(string path)
        {
            File.Delete(path);
        }

        private string GenerateTempLocation()
        {
            return (System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv");
        }
        #endregion
    }
}
