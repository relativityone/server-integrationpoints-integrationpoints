using System;
using System.Collections.Generic;
using System.Web;
using NUnit.Framework;
using NSubstitute;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using Relativity.DataExchange.Service;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests
{
    [TestFixture, Category("Unit"), Category("ImportProvider")]
    public class PreviewJobTests
    {
        private List<object> _arrList;
        private PreviewJob _subjectUnderTest;
        private const int _COLUMN_COUNT = 5;
        private const string _ALERT_SCRIPT = "<script>Alert(1)</script>";

        [SetUp]
        public void SetUp()
        {
            ILoadFilePreviewer loadFilePreviewerMock = Substitute.For<ILoadFilePreviewer>();
            _subjectUnderTest = new PreviewJob();
            _subjectUnderTest._loadFilePreviewer = loadFilePreviewerMock;
            _arrList = new List<object>();
        }

        [Test]
        public void PreviewJobReturnsProperHeaders()
        {
            MockPreviewTable();

            _subjectUnderTest._loadFilePreviewer.ReadFile(false).ReturnsForAnyArgs(_arrList);
            _subjectUnderTest.StartRead();
            //assert that Preview Job is set to complete
            Assert.IsTrue(_subjectUnderTest.IsComplete);

            //assert that we have the expected number of headers
            Assert.AreEqual(_COLUMN_COUNT, _subjectUnderTest.PreviewTable.Header.Count);
        }

        [Test]
        public void PreviewJobReturnsCorrectErrorsExceptions()
        {
            MockPreviewTable();
            //arrange our return value from ReadFile to include two exceptions
            _arrList.Add(new Exception("test 1"));
            _arrList.Add(new Exception("test 2"));
            _subjectUnderTest._loadFilePreviewer.ReadFile(false).ReturnsForAnyArgs(_arrList);
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
            _subjectUnderTest._loadFilePreviewer.ReadFile(false).ReturnsForAnyArgs(_arrList);
            _subjectUnderTest.StartRead();

            //assert that we have the expected number errors and data rows
            Assert.AreEqual(1, _subjectUnderTest.PreviewTable.ErrorRows.Count);
            Assert.AreEqual(2, _subjectUnderTest.PreviewTable.Data.Count);
        }

        [Test]
        public void PreviewJobEncodesValues()
        {
            MockPreviewTable();

            //arrange our return value from ReadFile to include an value with a script inside it
            _subjectUnderTest._loadFilePreviewer.ReadFile(false).ReturnsForAnyArgs(_arrList);
            _subjectUnderTest.StartRead();

            //assert that we have the expected number of rows
            Assert.AreEqual(1, _subjectUnderTest.PreviewTable.Data.Count);
            //Assert that the row and column that contained the Alert script has been safely html encoded
            string valueWithScript = _subjectUnderTest.PreviewTable.Data[0][1];
            Assert.AreEqual(HttpUtility.HtmlEncode(_ALERT_SCRIPT), valueWithScript);
        }

        [Test]
        public void PreviewJobReturnsNoData()
        {
            //Test to make sure that PreviewJob is safely handling a situation where LoadFilePreviewer doesn't find any rows
            _subjectUnderTest._loadFilePreviewer.ReadFile(false).ReturnsForAnyArgs(_arrList);
            _subjectUnderTest.StartRead();

            //assert that we have the expected number errors and data rows
            Assert.AreEqual(1, _subjectUnderTest.PreviewTable.Header.Count);

            //assert that the Read did not fail
            Assert.IsFalse(_subjectUnderTest.IsFailed);
        }

        private void MockPreviewTable()
        {
            FieldCategory fieldCat;
            WinEDDS.Api.ArtifactField field;
            List<WinEDDS.Api.ArtifactField> fieldList = new List<WinEDDS.Api.ArtifactField>();
            for (int i = 1; i <= _COLUMN_COUNT; i++)
            {
                fieldCat = FieldCategory.Generic;
                if (i == 1)
                {
                    fieldCat = FieldCategory.Identifier;
                }
                field = new WinEDDS.Api.ArtifactField(String.Format("Field{0}", i), i, FieldType.Text, fieldCat, -1, -1, -1, false);
                field.Value = String.Format("Value{0}",i);

                //Give one of these columns a value that contains a script
                if (i == 2)
                {
                    field.Value = _ALERT_SCRIPT;
                }
                fieldList.Add(field);
            }
            _arrList.Add(fieldList.ToArray());
        }

        private void MockPreviewTableErrorColumn()
        {
            FieldCategory fieldCat;
            WinEDDS.Api.ArtifactField field;
            List<WinEDDS.Api.ArtifactField> fieldList = new List<WinEDDS.Api.ArtifactField>();
            for (int i = 1; i <= _COLUMN_COUNT; i++)
            {
                fieldCat = FieldCategory.Generic;
                if (i == 1)
                {
                    fieldCat = FieldCategory.Identifier;
                }
                field = new WinEDDS.Api.ArtifactField(String.Format("Field{0}", i), i, FieldType.Text, fieldCat, -1, -1, -1, false);
                //insert value with the error keyword that the LoadFilePreviewer will put on columns that contain an error
                field.Value = String.Format("Error: {0}", i);
                fieldList.Add(field);
            }
            _arrList.Add(fieldList.ToArray());
        }
    }
}
