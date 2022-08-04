using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Common.Extensions.DotNet;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Services
{
    [TestFixture, Category("Unit")]
    public class ViewServiceTests : TestBase
    {
        private ViewService _sut;
        private ISearchService _searchServiceMock;

        private const int _WORKSPACE_ID = 12345;
        private const int _ARTIFACT_TYPE_ID = 10;

        private const string _ARTIFACT_ID_COL_NAME = "ArtifactId";
        private const string _NAME_COL_NAME = "Name";
        private const string _AVAILABLE_COL_NAME = "AvailableInObjectTab";
        private const string _ORDER_COL_NAME = "Order";

        private const int _VIEW_1_ARTIFACT_ID = 10;
        private const string _VIEW_1_NAME = "View1";
        private const bool _VIEW_1_AVAILABLE = true;
        private readonly int _VIEW_1_ORDER = 101;
        
        private const int _VIEW_2_ARTIFACT_ID = 20;
        private const string _VIEW_2_NAME = "View2";
        private const bool _VIEW_2_AVAILABLE = true;
        private readonly int _VIEW_2_ORDER = 10;

        private const int _VIEW_3_ARTIFACT_ID = 30;
        private const string _VIEW_3_NAME = "View3";
        private const bool _VIEW_3_AVAILABLE = true;
        private readonly int _VIEW_3_ORDER = 100;

        private const int _VIEW_4_ARTIFACT_ID = 40;
        private const string _VIEW_4_NAME = "View4";
        private const bool _VIEW_4_AVAILABLE = true;
        private readonly int _VIEW_4_ORDER = 99;

        private const int _VIEW_5_ARTIFACT_ID = 50;
        private const string _VIEW_5_NAME = "View5";
        private const bool _VIEW_5_AVAILABLE = false;
        private readonly int _VIEW_5_ORDER = 20;

        [SetUp]
        public override void SetUp()
        {
            _searchServiceMock = Substitute.For<ISearchService>();

            IHelper helper = Substitute.For<IHelper>();
            helper.GetServicesManager().CreateProxy<ISearchService>(ExecutionIdentity.CurrentUser).Returns(_searchServiceMock);

            _sut = new ViewService(helper);
        }

        [Test]
        public void ItShoulGetWorkspaceViewsSorted()
        {
            // Arrange
            _searchServiceMock.RetrieveViewsByContextArtifactIDAsync(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, false, string.Empty)
                .Returns(CreateDataSet());

            // Act
            List<ViewDTO> views = _sut.GetViewsByWorkspaceAndArtifactType(_WORKSPACE_ID, _ARTIFACT_TYPE_ID);

            // Assert
            Assert.That(views.IsNullOrEmpty(), Is.Not.Null);
            Assert.That(views.Count, Is.EqualTo(4));

            Assert.That(views[0].ArtifactId, Is.EqualTo(_VIEW_2_ARTIFACT_ID));
            Assert.That(views[0].Name, Is.EqualTo(_VIEW_2_NAME));
            Assert.That(views[0].IsAvailableInObjectTab, Is.EqualTo(_VIEW_2_AVAILABLE));
            Assert.That(views[0].Order, Is.EqualTo(_VIEW_2_ORDER));

            Assert.That(views[1].ArtifactId, Is.EqualTo(_VIEW_4_ARTIFACT_ID));
            Assert.That(views[1].Name, Is.EqualTo(_VIEW_4_NAME));
            Assert.That(views[1].IsAvailableInObjectTab, Is.EqualTo(_VIEW_4_AVAILABLE));
            Assert.That(views[1].Order, Is.EqualTo(_VIEW_4_ORDER));

            Assert.That(views[2].ArtifactId, Is.EqualTo(_VIEW_3_ARTIFACT_ID));
            Assert.That(views[2].Name, Is.EqualTo(_VIEW_3_NAME));
            Assert.That(views[2].IsAvailableInObjectTab, Is.EqualTo(_VIEW_3_AVAILABLE));
            Assert.That(views[2].Order, Is.EqualTo(_VIEW_3_ORDER));

            Assert.That(views[3].ArtifactId, Is.EqualTo(_VIEW_1_ARTIFACT_ID));
            Assert.That(views[3].Name, Is.EqualTo(_VIEW_1_NAME));
            Assert.That(views[3].IsAvailableInObjectTab, Is.EqualTo(_VIEW_1_AVAILABLE));
            Assert.That(views[3].Order, Is.EqualTo(_VIEW_1_ORDER));
        }

        [Test]
        public void ItShouldThrowException()
        {
            // Arrange
            _searchServiceMock.RetrieveViewsByContextArtifactIDAsync(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, false, string.Empty)
                .Returns(new DataSetWrapper(new DataSet()));

            // Act & Assert
            Assert.Throws<Exception>(() => _sut.GetViewsByWorkspaceAndArtifactType(_WORKSPACE_ID, _ARTIFACT_TYPE_ID));
        }

        private DataSetWrapper CreateDataSet()
        {
            DataSet ds = new DataSet();
            DataTable dt = ds.Tables.Add("Table");

            dt.Columns.Add(_ARTIFACT_ID_COL_NAME, typeof(int));
            dt.Columns.Add(_NAME_COL_NAME, typeof(string));
            dt.Columns.Add(_AVAILABLE_COL_NAME, typeof(bool));
            dt.Columns.Add(_ORDER_COL_NAME, typeof(int));

            AddRow(dt, _VIEW_1_ARTIFACT_ID, _VIEW_1_NAME, _VIEW_1_AVAILABLE, _VIEW_1_ORDER);
            AddRow(dt, _VIEW_2_ARTIFACT_ID, _VIEW_2_NAME, _VIEW_2_AVAILABLE, _VIEW_2_ORDER);
            AddRow(dt, _VIEW_3_ARTIFACT_ID, _VIEW_3_NAME, _VIEW_3_AVAILABLE, _VIEW_3_ORDER);
            AddRow(dt, _VIEW_4_ARTIFACT_ID, _VIEW_4_NAME, _VIEW_4_AVAILABLE, _VIEW_4_ORDER);
            AddRow(dt, _VIEW_5_ARTIFACT_ID, _VIEW_5_NAME, _VIEW_5_AVAILABLE, _VIEW_5_ORDER);

            return new DataSetWrapper(ds);
        }

        private void AddRow(DataTable dt, int artifactId, string name, bool available, int order)
        {
            DataRow newRow = dt.NewRow();
            newRow[_ARTIFACT_ID_COL_NAME] = artifactId;
            newRow[_NAME_COL_NAME] = name;
            newRow[_AVAILABLE_COL_NAME] = available;
            newRow[_ORDER_COL_NAME] = order;
            dt.Rows.Add(newRow);
        }
    }
}
