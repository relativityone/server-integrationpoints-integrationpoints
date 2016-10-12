
using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Services;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.Utility.Extensions;
using kCura.WinEDDS.Service.Export;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Services
{
	public class ViewServiceTests
	{
		private ViewService _subjectUnderTest;
		private IServiceManagerProvider _serviceManagerProviderMock;
		private ISearchManager _searchManagerMock;

		private const int _WORKSPACE_ID = 12345;
		private const int _ARTIFACT_TYPE_ID = 10;

		private const string _ARTIFACT_ID_COL_NAME = "ArtifactId";
		private const string _NAME_COL_NAME = "Name";
		private const string _AVAILABLE_COL_NAME = "AvailableInObjectTab";

		private const int _VIEW_1_ARTIFACT_ID = 10;
		private const string _VIEW_1_NAME = "View1";
		private const bool _VIEW_1_AVAILABLE = false;

		private const int _VIEW_2_ARTIFACT_ID = 20;
		private const string _VIEW_2_NAME = "View2";
		private const bool _VIEW_2_AVAILABLE = true;

		[SetUp]
		public void SetUp()
		{
			var helper = Substitute.For<IHelper>();
			_searchManagerMock = Substitute.For<ISearchManager>();

			_serviceManagerProviderMock = Substitute.For<IServiceManagerProvider>();
			_serviceManagerProviderMock.Create<ISearchManager, SearchManagerFactory>().Returns(_searchManagerMock);

			_subjectUnderTest = new ViewService(_serviceManagerProviderMock, helper);
		}

		[Test]
		public void ItShoulGetWorkspaceViews()
		{
			// Arrange
			_searchManagerMock.RetrieveViewsByContextArtifactID(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, false)
				.Returns(CreateDataSet());

			// Act
			List<ViewDTO> _views = _subjectUnderTest.GetViewsByWorkspaceAndArtifactType(_WORKSPACE_ID, _ARTIFACT_TYPE_ID);

			// Assert
			Assert.That(_views.IsNullOrEmpty(), Is.Not.Null);
			Assert.That(_views.Count, Is.EqualTo(1));

			ViewDTO viewDto = _views[0];

			Assert.That(viewDto.ArtifactId, Is.EqualTo(_VIEW_2_ARTIFACT_ID));
			Assert.That(viewDto.Name, Is.EqualTo(_VIEW_2_NAME));
			Assert.That(viewDto.IsAvailableInObjectTab, Is.EqualTo(_VIEW_2_AVAILABLE));
		}

		[Test]
		public void ItShouldThrowException()
		{
			// Arrange
			_searchManagerMock.RetrieveViewsByContextArtifactID(_WORKSPACE_ID, _ARTIFACT_TYPE_ID, false).Returns(new DataSet());

			// Act & Assert
			Assert.Throws<Exception>(() => _subjectUnderTest.GetViewsByWorkspaceAndArtifactType(_WORKSPACE_ID, _ARTIFACT_TYPE_ID));
		}

		private DataSet CreateDataSet()
		{
			DataSet ds = new DataSet();
			DataTable dt = ds.Tables.Add("Table");

			dt.Columns.Add(_ARTIFACT_ID_COL_NAME, typeof(int));
			dt.Columns.Add(_NAME_COL_NAME, typeof(string));
			dt.Columns.Add(_AVAILABLE_COL_NAME, typeof(bool));

			DataRow rowFirst = dt.NewRow();
			rowFirst[_ARTIFACT_ID_COL_NAME] = _VIEW_1_ARTIFACT_ID;
			rowFirst[_NAME_COL_NAME] = _VIEW_1_NAME;
			rowFirst[_AVAILABLE_COL_NAME] = _VIEW_1_AVAILABLE;
			dt.Rows.Add(rowFirst);

			DataRow rowSecond = dt.NewRow();
			rowSecond[_ARTIFACT_ID_COL_NAME] = _VIEW_2_ARTIFACT_ID;
			rowSecond[_NAME_COL_NAME] = _VIEW_2_NAME;
			rowSecond[_AVAILABLE_COL_NAME] = _VIEW_2_AVAILABLE;
			dt.Rows.Add(rowSecond);

			return ds;
		}
	}
}
