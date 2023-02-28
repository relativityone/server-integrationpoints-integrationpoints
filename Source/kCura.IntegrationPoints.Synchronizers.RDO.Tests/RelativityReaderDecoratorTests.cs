using System;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests
{
    [TestFixture, Category("Unit")]
    public class RelativityReaderDecoratorTests : TestBase
    {
        IDataReader _reader;
        FieldMap[] _fieldMaps;
        private const string _Column0Name = "123";
        private const string _Column1Name = "546";
        private const string _Column2Name = "789";
        private const string _Column3Name = "987";
        private const string _DestinationColumn0Name = "Id";
        private const string _DestinationColumn1Name = "The first second field";
        private const string _DestinationColumn2Name = "The first third field";
        private const string _ExpectNativeFilePath = @"C:\abc\def.txt";
        private const string _ExpectFolderInformationPath = @"C:\abc\def.txt";

        [SetUp]
        public override void SetUp()
        {
            DataTable table = new DataTable();
            table.Columns.AddRange(new[] { new DataColumn(_Column0Name, typeof(int)),
                new DataColumn(_Column1Name),
                new DataColumn(_Column2Name),
                new DataColumn(_Column3Name),
                new DataColumn(kCura.IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD) });
            table.Rows.Add(1, "ABC", "EFG", _ExpectNativeFilePath, _ExpectFolderInformationPath);
            table.Rows.Add(2, "Name", "DataTable");
            table.Rows.Add(3, "9", "Testing");
            table.Rows.Add(DBNull.Value, "8", "Testing2");
            table.Rows.Add(5, null, DBNull.Value);
            _reader = new DataTableReader(table);

            _fieldMaps = new[]
            {
                new FieldMap()
                {
                    DestinationField = new FieldEntry()
                    {
                        DisplayName = _DestinationColumn0Name + " [Object Identifier]",
                        FieldIdentifier = 1.ToString(),
                        FieldType = FieldType.String,
                        IsIdentifier = true
                    },
                    FieldMapType = FieldMapTypeEnum.Identifier,
                    SourceField = new FieldEntry()
                    {
                        DisplayName = "Another Id",
                        FieldIdentifier = _Column0Name,
                        FieldType = FieldType.String,
                        IsIdentifier = true
                    }
                },
                new FieldMap()
                {
                    DestinationField = new FieldEntry()
                    {
                        DisplayName = _DestinationColumn1Name,
                        FieldIdentifier = 2.ToString(),
                        FieldType = FieldType.String,
                        IsIdentifier = true
                    },
                    FieldMapType = FieldMapTypeEnum.None,
                    SourceField = new FieldEntry()
                    {
                        DisplayName = "Another second field",
                        FieldIdentifier = _Column1Name,
                        FieldType = FieldType.String,
                        IsIdentifier = true
                    }
                },
                new FieldMap()
                {
                    DestinationField = new FieldEntry()
                    {
                        DisplayName = _DestinationColumn2Name,
                        FieldIdentifier = 3.ToString(),
                        FieldType = FieldType.String,
                        IsIdentifier = true
                    },
                    FieldMapType = FieldMapTypeEnum.None,
                    SourceField = new FieldEntry()
                    {
                        DisplayName = "Another third field",
                        FieldIdentifier = _Column2Name,
                        FieldType = FieldType.String,
                        IsIdentifier = true
                    }
                },
                new FieldMap()
                {
                    DestinationField = new FieldEntry(),
                    FieldMapType = FieldMapTypeEnum.NativeFilePath,
                    SourceField = new FieldEntry()
                    {
                        DisplayName = "Native File Path",
                        FieldIdentifier = _Column3Name,
                        FieldType = FieldType.String,
                        IsIdentifier = true
                    }
                },
                new FieldMap()
                {
                    DestinationField = new FieldEntry(),
                    FieldMapType = FieldMapTypeEnum.FolderPathInformation,
                    SourceField = new FieldEntry()
                    {
                        DisplayName = kCura.IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD_NAME,
                        FieldIdentifier = kCura.IntegrationPoints.Domain.Constants.SPECIAL_FOLDERPATH_FIELD,
                        FieldType = FieldType.String,
                        IsIdentifier = false
                    }
                }
            };
        }

        [Test]
        public void ReaderDecoratorFieldCountReflectsTheFieldsInTheActualReader()
        {
            RelativityReaderDecorator decorator = new RelativityReaderDecorator(_reader, _fieldMaps);
            Assert.AreEqual(_reader.FieldCount, decorator.FieldCount);
        }

        [Test]
        public void ReaderDecoratorCloseTheActualReaderProperly()
        {
            RelativityReaderDecorator decorator = new RelativityReaderDecorator(_reader, _fieldMaps);
            Assert.IsFalse(decorator.IsClosed);

            decorator.Close();

            Assert.AreEqual(_reader.IsClosed, decorator.IsClosed);
            Assert.IsTrue(decorator.IsClosed);
        }

        [Test]
        public void ReaderDecoratorCanReadTheActualReaderProperly()
        {
            RelativityReaderDecorator decorator = new RelativityReaderDecorator(_reader, _fieldMaps);
            Assert.IsTrue(decorator.Read());

            int id = decorator.GetInt32(0);
            string column1 = decorator.GetString(1);
            string column2 = decorator.GetValue(2) as string;

            Assert.AreEqual(1, id);
            Assert.AreEqual("ABC", column1);
            Assert.AreEqual("EFG", column2);
        }

        [Test]
        public void ReaderDecoratorCanReadTheActualReaderByUsingTheNameOfTheDestination()
        {
            RelativityReaderDecorator decorator = new RelativityReaderDecorator(_reader, _fieldMaps);
            Assert.IsTrue(decorator.Read());

            int id = (int)decorator[_DestinationColumn0Name];
            string column1 = decorator[_DestinationColumn1Name] as string;
            string column2 = decorator[_DestinationColumn2Name] as string;

            Assert.AreEqual(1, id);
            Assert.AreEqual("ABC", column1);
            Assert.AreEqual("EFG", column2);
        }

        [Test]
        public void ReaderDecoratorWillThrowExceptionWhenIdentifierFieldIsNotSet()
        {
            RelativityReaderDecorator decorator = new RelativityReaderDecorator(_reader, _fieldMaps);
            Assert.IsTrue(decorator.Read());
            Assert.IsTrue(decorator.Read());
            Assert.IsTrue(decorator.Read());
            Assert.IsTrue(decorator.Read());

            Assert.Throws<IndexOutOfRangeException>(() => { int x = (int) decorator[0]; }, "Identifier["+ _DestinationColumn0Name + "] must have a value.");
        }

        [Test]
        public void ReaderDecoratorWillNotThrowExceptionWhenNonIdentifierFieldIsNotSet()
        {
            RelativityReaderDecorator decorator = new RelativityReaderDecorator(_reader, _fieldMaps);
            Assert.IsTrue(decorator.Read());
            Assert.IsTrue(decorator.Read());
            Assert.IsTrue(decorator.Read());
            Assert.IsTrue(decorator.Read());

            Assert.DoesNotThrow(() => { string x = (string)decorator[1]; });
            Assert.DoesNotThrow(() => { string x = (string)decorator[2]; });
        }

        [Test]
        public void ReaderDecoratorWillThrowIndexOutOfBoundExceptionWhenPassInInvalidOrdinal()
        {
            RelativityReaderDecorator decorator = new RelativityReaderDecorator(_reader, _fieldMaps);
            Assert.IsTrue(decorator.Read());

            Assert.Throws<IndexOutOfRangeException>(() => { string x = (string)decorator[999]; });
        }

        [Test]
        public void ReaderDecoratorWillThrowIndexOutOfBoundExceptionWhenPassInInvalidColumnNameInGetOrdinal()
        {
            RelativityReaderDecorator decorator = new RelativityReaderDecorator(_reader, _fieldMaps);
            Assert.IsTrue(decorator.Read());

            Assert.Throws<IndexOutOfRangeException>(() => { int x =  decorator.GetOrdinal("hello world"); });
        }

        [Test]
        public void ReaderDecoratorWillThrowIndexOutOfBoundExceptionWhenPassInInvalidColumnNameInOverriddenOperator()
        {
            RelativityReaderDecorator decorator = new RelativityReaderDecorator(_reader, _fieldMaps);
            Assert.IsTrue(decorator.Read());

            Assert.Throws<IndexOutOfRangeException>(() => { int x = (int)decorator["hello world"]; });
        }

        [Test]
        public void ReaderDecoratorMapNativeFilePathToASpecialField()
        {
            RelativityReaderDecorator decorator = new RelativityReaderDecorator(_reader, _fieldMaps);
            Assert.IsTrue(decorator.Read());

            string nativeFilePath = (string)decorator[3];
            Assert.AreEqual(_ExpectNativeFilePath, nativeFilePath);
        }

        [Test]
        public void ReaderDecoratorMapNativeFilePathToASpecialField2()
        {
            RelativityReaderDecorator decorator = new RelativityReaderDecorator(_reader, _fieldMaps);
            Assert.IsTrue(decorator.Read());

            string nativeFilePath = (string)decorator["NATIVE_FILE_PATH_001"];
            Assert.AreEqual(_ExpectNativeFilePath, nativeFilePath);
        }

        [Test]
        public void ReaderDecoratorMapFolderInformationPath()
        {
            RelativityReaderDecorator decorator = new RelativityReaderDecorator(_reader, _fieldMaps);
            Assert.IsTrue(decorator.Read());

            string folderInformationPath = (string)decorator[_fieldMaps[4].SourceField.DisplayName];
            Assert.AreEqual(_ExpectFolderInformationPath, folderInformationPath);
        }
    }
}
