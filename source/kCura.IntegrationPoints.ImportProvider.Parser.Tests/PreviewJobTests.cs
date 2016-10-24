using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using kCura.WinEDDS;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
    [TestFixture, Category("ImportProvider")]
    public class PreviewJobTests
    {
        private const int ColumnCount = 5;
        private ArrayList arrList;
        private ILoadFilePreviewer _loadFilePreviewerMock;
        private PreviewJob _subjectUnderTest;

        [SetUp]
        public void SetUp()
        {
            _loadFilePreviewerMock = Substitute.For<ILoadFilePreviewer>();
            _subjectUnderTest = new PreviewJob();
            _subjectUnderTest._loadFilePreviewer = _loadFilePreviewerMock;
            arrList = new ArrayList();
        }

        [Test]
        public void PreviewJobReturnsProperHeaders()
        {
            MockPreviewTable();

            _subjectUnderTest._loadFilePreviewer.ReadFile().ReturnsForAnyArgs(arrList);
            _subjectUnderTest.StartRead();
            //assert that Preview Job is set to complete
            Assert.IsTrue(_subjectUnderTest.IsComplete);

            //assert that we have the expected number of headers
            Assert.AreEqual(ColumnCount, _subjectUnderTest.PreviewTable.Header.Count);
        }

        [Test]
        public void PreviewJobReturnsCorrectErrorsExceptions()
        {
            MockPreviewTable();
            //arrange our return value from ReadFile to include two exceptions
            arrList.Add(new Exception("test 1"));
            arrList.Add(new Exception("test 2"));
            _subjectUnderTest._loadFilePreviewer.ReadFile().ReturnsForAnyArgs(arrList);
            _subjectUnderTest.StartRead();

            //assert that we have the expected number errors
            Assert.AreEqual(2, _subjectUnderTest.PreviewTable.ErrorRows.Count);
        }

        [Test]
        public void PreviewJobReturnsCorrectErrors()
        {
            MockPreviewTable();
            MockPreviewTableErrorColumn();
            
            //arrange our return value from ReadFile to include an exception column
            //this returns differently than when the entire row has an exception
            _subjectUnderTest._loadFilePreviewer.ReadFile().ReturnsForAnyArgs(arrList);
            _subjectUnderTest.StartRead();

            //assert that we have the expected number errors and data rows
            Assert.AreEqual(1, _subjectUnderTest.PreviewTable.ErrorRows.Count);
            Assert.AreEqual(2, _subjectUnderTest.PreviewTable.Data.Count);
        }

        [Test]
        public void PreviewJobReturnsNoData()
        {
            //Test to make sure that PreviewJob is safely handling a situation where LoadFilePreviewer doesn't find any rows
            _subjectUnderTest._loadFilePreviewer.ReadFile().ReturnsForAnyArgs(arrList);
            _subjectUnderTest.StartRead();

            //assert that we have the expected number errors and data rows
            Assert.AreEqual(1, _subjectUnderTest.PreviewTable.Header.Count);

            //assert that the Read did not fail
            Assert.IsFalse(_subjectUnderTest.IsFailed);
        }

        private void MockPreviewTable()
        {
            global::Relativity.FieldCategory fieldCat;            
            WinEDDS.Api.ArtifactField field;
            List<WinEDDS.Api.ArtifactField> fieldList = new List<WinEDDS.Api.ArtifactField>();
            for (int i = 1; i <= ColumnCount; i++)
            {
                fieldCat = global::Relativity.FieldCategory.Generic;
                if (i == 1)
                {
                    fieldCat = global::Relativity.FieldCategory.Identifier;
                }
                field = new WinEDDS.Api.ArtifactField(String.Format("Field{0}", i), i, global::Relativity.FieldTypeHelper.FieldType.Text, fieldCat, -1, -1, -1, false);
                field.Value = String.Format("Value{0}",i);
                fieldList.Add(field);
            }
            arrList.Add(fieldList.ToArray());
        }

        private void MockPreviewTableErrorColumn()
        {
            global::Relativity.FieldCategory fieldCat;
            WinEDDS.Api.ArtifactField field;
            List<WinEDDS.Api.ArtifactField> fieldList = new List<WinEDDS.Api.ArtifactField>();
            for (int i = 1; i <= ColumnCount; i++)
            {
                fieldCat = global::Relativity.FieldCategory.Generic;
                if (i == 1)
                {
                    fieldCat = global::Relativity.FieldCategory.Identifier;
                }
                field = new WinEDDS.Api.ArtifactField(String.Format("Field{0}", i), i, global::Relativity.FieldTypeHelper.FieldType.Text, fieldCat, -1, -1, -1, false);
                //insert value with the error keyword that the LoadFilePreviewer will put on columns that contain an error
                field.Value = String.Format("Error: {0}", i);
                fieldList.Add(field);
            }
            arrList.Add(fieldList.ToArray());
        }
    }
}
