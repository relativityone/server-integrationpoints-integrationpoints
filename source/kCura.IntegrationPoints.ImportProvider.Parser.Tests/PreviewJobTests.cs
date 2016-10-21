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
        private ILoadFilePreviewer _loadFilePreviewerMock;
        private PreviewJob _subjectUnderTest;

        [SetUp]
        public void SetUp()
        {
            _loadFilePreviewerMock = Substitute.For<ILoadFilePreviewer>();
            _subjectUnderTest = new PreviewJob();
            _subjectUnderTest._loadFilePreviewer = _loadFilePreviewerMock;
        }

        [Test]
        public void ImportJobReturnsProperHeaders()
        {
            MockPreviewTable();

            _subjectUnderTest.StartRead();
            //assert that Preview Job is set to complete
            Assert.IsTrue(_subjectUnderTest.IsComplete);

            //assert that we have the expected number of headers
            Assert.AreEqual(ColumnCount, _subjectUnderTest.PreviewTable.Header.Count);
        }

        private void MockPreviewTable()
        {
            global::Relativity.FieldCategory fieldCat;
            ArrayList arrList = new ArrayList();
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
            _subjectUnderTest._loadFilePreviewer.ReadFile().ReturnsForAnyArgs(arrList);
        }
    }
}
