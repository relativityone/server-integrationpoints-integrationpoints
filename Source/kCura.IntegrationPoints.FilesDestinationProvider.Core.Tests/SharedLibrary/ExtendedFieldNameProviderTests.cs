using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using NUnit.Framework;
using Relativity.DataExchange.Service;
using Relativity.IntegrationPoints.Contracts.Models;
using ViewFieldInfo = kCura.WinEDDS.ViewFieldInfo;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.SharedLibrary
{
    [TestFixture, Category("Unit")]
    public class ExtendedFieldNameProviderTest : TestBase
    {
        private ExtendedFieldNameProvider _extendedFieldNameProvider;
        private ExportSettings _settings;
        private DataRow _dr;

        public override void SetUp()
        {
            _settings = new ExportSettings();
            DataTable dt = new DataTable("TestTable");
            _dr = dt.NewRow();

            InsertColumnWithValue(dt, _dr, "TestFieldId", "TestValue");
            InsertColumnWithValue(dt, _dr, "FieldArtifactId", 12345);
            InsertColumnWithValue(dt, _dr, "AvfId", "1");
            InsertColumnWithValue(dt, _dr, "FieldCategoryID", 2);
            InsertColumnWithValue(dt, _dr, "DisplayName", "TestDisplayName");
            InsertColumnWithValue(dt, _dr, "AvfColumnName", "AvfTestColumnName");
            InsertColumnWithValue(dt, _dr, "AvfHeaderName", "AvfTestHeaderName");
            InsertColumnWithValue(dt, _dr, "AllowFieldName", "TestAllowFieldName");
            InsertColumnWithValue(dt, _dr, "ColumnSource", ColumnSourceType.Artifact);
            InsertColumnWithValue(dt, _dr, "DataSource", "TestDataSource");
            InsertColumnWithValue(dt, _dr, "SourceFieldDisplayName", "TestSourceFieldDisplayName");
            InsertColumnWithValue(dt, _dr, "SourceFieldArtifactTypeID", 666);
            InsertColumnWithValue(dt, _dr, "ConnectorFieldArtifactID", 666);
            InsertColumnWithValue(dt, _dr, "SourceFieldArtifactTypeTableName", "TestSourceFieldArtifactTypeTableName");
            InsertColumnWithValue(dt, _dr, "ConnectorFieldName", "TestConnectorFieldName");
            InsertColumnWithValue(dt, _dr, "FieldTypeID", global::Relativity.IntegrationPoints.Contracts.Models.FieldType.String);
            InsertColumnWithValue(dt, _dr, "ConnectorFieldCategoryID", FieldCategory.Generic);
            InsertColumnWithValue(dt, _dr, "IsLinked", false);
            InsertColumnWithValue(dt, _dr, "FieldCodeTypeID", 666);
            InsertColumnWithValue(dt, _dr, "ArtifactTypeID", 666);
            InsertColumnWithValue(dt, _dr, "ArtifactTypeTableName", "TestArtifactTypeTableName");
            InsertColumnWithValue(dt, _dr, "FieldIsArtifactBaseField", false);
            InsertColumnWithValue(dt, _dr, "FormatString", "");
            InsertColumnWithValue(dt, _dr, "IsUnicodeEnabled", false);
            InsertColumnWithValue(dt, _dr, "AllowHtml", false);
            InsertColumnWithValue(dt, _dr, "ParentFileFieldArtifactID", 666);
            InsertColumnWithValue(dt, _dr, "ParentFileFieldDisplayName", "TestParentFileFieldDisplayName");
            InsertColumnWithValue(dt, _dr, "AssociativeArtifactTypeID", 666);
            InsertColumnWithValue(dt, _dr, "RelationalTableName", "");
            InsertColumnWithValue(dt, _dr, "RelationalTableColumnName", "TestRelationalTableColumnName");
            InsertColumnWithValue(dt, _dr, "RelationalTableColumnName2", "TestRelationalTableColumnName2");
            InsertColumnWithValue(dt, _dr, "SourceFieldArtifactID", 666);
            InsertColumnWithValue(dt, _dr, "EnableDataGrid", false);
        }

        [Test]
        public void ItShouldChangeDisplayNameWhenSettingsExist()
        {
            _settings.SelViewFieldIds = new Dictionary<int, FieldEntry>();
            _settings.SelViewFieldIds.Add(1, new FieldEntry() { DisplayName = "Test field", FieldIdentifier = "TestFieldId" });
            _extendedFieldNameProvider = new ExtendedFieldNameProvider(_settings);

            var viewFieldInfo = new ViewFieldInfo(_dr);
            string displayName = _extendedFieldNameProvider.GetDisplayName(viewFieldInfo);
            Assert.AreEqual("Test field", displayName);

        }

        [Test]
        public void ItShouldNotChangeDisplayNameWhenColumnDoesNotExistInSettings()
        {
            _extendedFieldNameProvider = new ExtendedFieldNameProvider(_settings);

            var viewFieldInfo = new ViewFieldInfo(_dr);
            string displayName = _extendedFieldNameProvider.GetDisplayName(viewFieldInfo);
            Assert.AreNotEqual("Test field", displayName);

        }

        [Test]
        public void ItShouldNotChangeDisplayNameWhenSettingsAreEmpty()
        {
            _extendedFieldNameProvider = new ExtendedFieldNameProvider(_settings);

            var viewFieldInfo = new ViewFieldInfo(_dr);
            string displayName = _extendedFieldNameProvider.GetDisplayName(viewFieldInfo);
            Assert.AreNotEqual("Test field", displayName);

        }

        [Test]
        public void ItShouldNotChangeNameForTextPrecedenceFields()
        {
            _settings.SelViewFieldIds = new Dictionary<int, FieldEntry>();
            _settings.SelViewFieldIds.Add(1, new FieldEntry() { DisplayName = "Test field", FieldIdentifier = "TestFieldId" });
            _extendedFieldNameProvider = new ExtendedFieldNameProvider(_settings);

            var textPrecedenceViewFieldInfo = new CoalescedTextViewField(new ViewFieldInfo(_dr), false);
            string displayName = _extendedFieldNameProvider.GetDisplayName(textPrecedenceViewFieldInfo);
            Assert.AreEqual(IntegrationPoints.Core.Constants.Export.TEXT_PRECEDENCE_AWARE_AVF_COLUMN_NAME, displayName);

        }

        private static void InsertColumnWithValue(DataTable dt, DataRow dr, string columnName, object rowValue)
        {
            dt.Columns.Add(new DataColumn(columnName, rowValue.GetType()));
            dr[columnName] = rowValue;
        }
    }
}
