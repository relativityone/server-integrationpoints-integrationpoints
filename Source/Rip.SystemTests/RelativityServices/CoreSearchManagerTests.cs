using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.Implementations;
using kCura.Relativity.Client;
using kCura.WinEDDS;
using Moq;
using NUnit.Framework;
using Relativity.API.Foundation;
using Relativity.Core;
using Relativity.Services.FileField;
using Relativity.Services.Interfaces.File;
using Relativity.Services.Interfaces.ViewField;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.User;
using Relativity.Services.View;
using FieldCategory = Relativity.Services.Objects.DataContracts.FieldCategory;
using FieldRef = Relativity.Services.Field.FieldRef;
using IFileRepository = kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories.IFileRepository;
using IViewManager = Relativity.Services.View.IViewManager;

namespace Rip.SystemTests.RelativityServices
{
	[TestFixture]
	public class CoreSearchManagerTests
	{
		private Lazy<ITestHelper> _testHelperLazy;
		private IRelativityObjectManager _objectManager;
		private IViewManager _viewManager;
		private int _workspaceID;

		private const int _DOCUMENT_ARTIFACT_TYPE_ID = 10;

		[OneTimeSetUp]
		public void OneSetup()
		{
			_workspaceID = SystemTestsFixture.WorkspaceID;
			_testHelperLazy = new Lazy<ITestHelper>(() => new TestHelper());
			IRelativityObjectManagerFactory objectManagerFactory = new RelativityObjectManagerFactory(_testHelperLazy.Value);
			_objectManager = objectManagerFactory.CreateRelativityObjectManager(_workspaceID);
			_viewManager = _testHelperLazy.Value.CreateUserProxy<IViewManager>();
		}

		[Test]
		public void RetrieveAllExportableViewFields_ShouldRetrieveAllViewFieldsForDocument()
		{
			// arrange
			CoreSearchManager sut = CreateCoreSearchManager();
			IList<int> exportableFieldIDs = RetrieveExportableFieldIDs();

			// act
			ViewFieldInfo[] result = sut.RetrieveAllExportableViewFields(_workspaceID, _DOCUMENT_ARTIFACT_TYPE_ID);

			// assert
			result.Length.Should().Be(exportableFieldIDs.Count);
			foreach (var viewFieldInfo in result)
			{
				exportableFieldIDs.Contains(viewFieldInfo.FieldArtifactId).Should().BeTrue();
			}
		}

		[Test]
		public void RetrieveDefaultViewFieldIDs_ShouldRetrieveDefaultViewFieldIDsForSavedSearchView()
		{
			// arrange
			CoreSearchManager sut = CreateCoreSearchManager();
			View view = CreateTestView();

			// act
			int[] result =
				sut.RetrieveDefaultViewFieldIds(_workspaceID, view.ArtifactID, _DOCUMENT_ARTIFACT_TYPE_ID, false);

			// assert
			view.Fields.Count.Should().Be(result.Length);
			var expectedViewFieldIDs = view.Fields.Select(fieldRef => fieldRef.ViewFieldID);
			result.Should().Contain(expectedViewFieldIDs);
		}

		private IList<int> RetrieveExportableFieldIDs()
		{
			var fieldQuery = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int) ArtifactType.Field },
				Fields = new[]
				{
					new Relativity.Services.Objects.DataContracts.FieldRef {Name = TestConstants.FieldNames.ARTIFACT_ID},
					new Relativity.Services.Objects.DataContracts.FieldRef {Name = TestConstants.FieldNames.FIELD_TYPE},
					new Relativity.Services.Objects.DataContracts.FieldRef {Name = TestConstants.FieldNames.FIELD_CATEGORY_ID}
				},
				Condition = $"'{TestConstants.FieldNames.OBJECT_TYPE_ARTIFACT_TYPE_ID}' == OBJECT {_DOCUMENT_ARTIFACT_TYPE_ID}"
			};

			ResultSet<RelativityObject> resultSet = _objectManager.Query(fieldQuery, 0, 1000);

			IList<int> fields = resultSet.Items
				.Select(fieldObject => new
				{
					ArtifactID = (int) fieldObject[TestConstants.FieldNames.ARTIFACT_ID].Value,
					FieldType = fieldObject[TestConstants.FieldNames.FIELD_TYPE].Value.ToString(),
					CategoryID = (int) fieldObject[TestConstants.FieldNames.FIELD_CATEGORY_ID].Value
				})
				.Where(item => IsFieldExportable(item.FieldType, item.CategoryID))
				.Select(item => item.ArtifactID)
				.ToList();

			return fields;
		}

		private static bool IsFieldExportable(string fieldType, int fieldCategoryID)
		{
			if (fieldCategoryID == (int)FieldCategory.FileInfo)
			{
				return false;
			}

			if (fieldCategoryID == (int)FieldCategory.MultiReflected)
			{
				if (fieldType == TestConstants.FieldTypeNames.LONG_TEXT ||
				    fieldType == TestConstants.FieldTypeNames.MULTIPLE_CHOICE)
				{
					return false;
				}
			}

			return true;
		}

		private View CreateTestView()
		{
			View view = new View
			{
				Name = "CoreSearchManagerTestsView",
				ArtifactTypeID = _DOCUMENT_ARTIFACT_TYPE_ID,
				Owner = new UserRef(0),
				Order = 9999,
				VisibleInDropdown = false,
				Fields =
				{
					new FieldRef(TestConstants.FieldNames.CONTROL_NUMBER),
					new FieldRef(TestConstants.FieldNames.EXTRACTED_TEXT),
					new FieldRef(TestConstants.FieldNames.GROUP_IDENTIFIER)
				}
			};

			int viewID = _viewManager.CreateSingleAsync(_workspaceID, view).GetAwaiter().GetResult();
			return _viewManager.ReadSingleAsync(_workspaceID, viewID).GetAwaiter().GetResult();
		}

		private CoreSearchManager CreateCoreSearchManager()
		{
			var baseServiceContextMock = new Mock<BaseServiceContext>(); // TODO remove when CoreSearchManager has Relativity.Core dependencies removed
			IViewFieldManager viewFieldManager = _testHelperLazy.Value.CreateUserProxy<IViewFieldManager>();
			IFileManager fileManager = _testHelperLazy.Value.CreateUserProxy<IFileManager>();
			IFileFieldManager fileFieldManager = _testHelperLazy.Value.CreateUserProxy<IFileFieldManager>();
			IExternalServiceInstrumentationProvider instrumentationProvider =
				new ExternalServiceInstrumentationProviderWithoutJobContext(_testHelperLazy.Value.GetLoggerFactory().GetLogger());
			IViewFieldRepository viewFieldRepository = new ViewFieldRepository(viewFieldManager, instrumentationProvider);
			IFileRepository fileRepository = new FileRepository(fileManager, instrumentationProvider);
			IFileFieldRepository fileFieldRepository = new FileFieldRepository(fileFieldManager, instrumentationProvider);
			var coreSearchManager = new CoreSearchManager(
				baseServiceContextMock.Object,
				fileRepository,
				fileFieldRepository,
				viewFieldRepository
			);
			return coreSearchManager;
		}
	}

	internal class ArtifactRef : IArtifactRef
	{
		public int ArtifactID { get; set; }
		public IList<Guid> Guids { get; set; }

		public ArtifactRef(int artifactID)
		{
			ArtifactID = artifactID;
		}
	}
}
