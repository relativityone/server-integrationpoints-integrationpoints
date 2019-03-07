using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.UtilityDTO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.Relativity.Client;
using kCura.WinEDDS;
using Moq;
using NUnit.Framework;
using Relativity.API.Foundation;
using Relativity.Core;
using Relativity.Services.Interfaces.ViewField;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.User;
using Relativity.Services.View;
using FieldCategory = Relativity.Services.Objects.DataContracts.FieldCategory;
using FieldRef = Relativity.Services.Field.FieldRef;

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
			_testHelperLazy = new Lazy<ITestHelper>(() => new TestHelper());
			_workspaceID = Workspace.CreateWorkspace(Guid.NewGuid().ToString(), "Relativity Starter Template");

			IRelativityObjectManagerFactory objectManagerFactory = new RelativityObjectManagerFactory(_testHelperLazy.Value);
			_objectManager = objectManagerFactory.CreateRelativityObjectManager(_workspaceID);
			_viewManager = _testHelperLazy.Value.CreateUserProxy<IViewManager>();
		}
		
		[OneTimeTearDown]
		public void TearDown()
		{
			Workspace.DeleteWorkspace(_workspaceID);
		}

		[Test]
		public void RetrieveAllExportableViewFieldsTest()
		{
			// arrange
			CoreSearchManager coreSearchManager = CreateCoreSearchManager();
			IList<int> exportableFieldIDs = RetrieveExportableFieldIDs();

			// act
			ViewFieldInfo[] result = coreSearchManager.RetrieveAllExportableViewFields(_workspaceID, _DOCUMENT_ARTIFACT_TYPE_ID);

			// assert
			Assert.AreEqual(result.Length, exportableFieldIDs.Count);
			foreach (var viewFieldInfo in result)
			{
				Assert.IsTrue(exportableFieldIDs.Contains(viewFieldInfo.FieldArtifactId));
			}
		}

		private IList<int> RetrieveExportableFieldIDs()
		{
			var fieldQuery = new QueryRequest
			{
				ObjectType = new ObjectTypeRef {ArtifactTypeID = (int) ArtifactType.Field},
				Fields = new[]
				{
					new Relativity.Services.Objects.DataContracts.FieldRef {Name = TestConstants.FieldNames.ARTIFACT_ID},
					new Relativity.Services.Objects.DataContracts.FieldRef {Name = TestConstants.FieldNames.FIELD_TYPE},
					new Relativity.Services.Objects.DataContracts.FieldRef {Name = TestConstants.FieldNames.FIELD_CATEGORY_ID}
				},
				Condition = $"'{TestConstants.FieldNames.OBJECT_TYPE_ARTIFACT_TYPE_ID}' == OBJECT {_DOCUMENT_ARTIFACT_TYPE_ID}"
			};

			ResultSet<RelativityObject> resultSet = _objectManager.Query(fieldQuery, 0, 1000);

			IList<int> fields = new List<int>();

			foreach (var fieldObject in resultSet.Items)
			{
				string fieldType = fieldObject[TestConstants.FieldNames.FIELD_TYPE].Value.ToString();
				int fieldCategoryID = (int)fieldObject[TestConstants.FieldNames.FIELD_CATEGORY_ID].Value;

				if (IsFieldExportable(fieldType, fieldCategoryID))
				{
					int artifactID = (int) fieldObject[TestConstants.FieldNames.ARTIFACT_ID].Value;
					fields.Add(artifactID);
				}
			}

			return fields;
		}

		private static bool IsFieldExportable(string fieldType, int fieldCategoryID)
		{
			if (fieldCategoryID == (int) FieldCategory.FileInfo)
			{
				return false;
			}

			if (fieldCategoryID == (int) FieldCategory.MultiReflected)
			{
				if (fieldType == TestConstants.FieldTypeNames.LONG_TEXT ||
				    fieldType == TestConstants.FieldTypeNames.MULTIPLE_CHOICE)
				{
					return false;
				}
			}

			return true;
		}

		[Test]
		public void RetrieveDefaultViewFieldIdsTest()
		{
			// arrange
			CoreSearchManager coreSearchManager = CreateCoreSearchManager();
			Relativity.Services.View.View view = CreateTestView();

			// act
			int[] result =
				coreSearchManager.RetrieveDefaultViewFieldIds(_workspaceID, view.ArtifactID, _DOCUMENT_ARTIFACT_TYPE_ID, false);

			// assert
			Assert.AreEqual(view.Fields.Count, result.Length);
			foreach (var fieldRef in view.Fields)
			{
				int artifactViewFieldID = fieldRef.ViewFieldID;
				Assert.IsTrue(result.Contains(artifactViewFieldID));
			}
		}

		private Relativity.Services.View.View CreateTestView()
		{
			Relativity.Services.View.View view = new Relativity.Services.View.View
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
			IExternalServiceInstrumentationProvider instrumentationProvider =
				new ExternalServiceInstrumentationProviderWithoutJobContext(_testHelperLazy.Value.GetLoggerFactory().GetLogger());
			IViewFieldRepository viewFieldRepository = new ViewFieldRepository(viewFieldManager, instrumentationProvider);
			var coreSearchManager = new CoreSearchManager(baseServiceContextMock.Object, viewFieldRepository);
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
