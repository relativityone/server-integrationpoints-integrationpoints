using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.Shared.Models;

namespace Rip.SystemTests.IntegrationPointServices
{
	[TestFixture]
	public class IntegrationPointServiceTests
	{
		private int _sourceWorkspaceID => SystemTestsFixture.WorkspaceID;
		private int _destinationWorkspaceID => SystemTestsFixture.DestinationWorkspaceID;
		private IWindsorContainer _container => SystemTestsFixture.Container;
		private ITestHelper _testHelper => SystemTestsFixture.TestHelper;
		private IIntegrationPointService _integrationPointService;
		private IRepositoryFactory _repositoryFactory;
		private IFieldManager _fieldManager;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_integrationPointService = _container.Resolve<IIntegrationPointService>();
			_repositoryFactory = _container.Resolve<IRepositoryFactory>();
			_fieldManager = _testHelper.CreateUserProxy<IFieldManager>();
		}

		[Test]
		public void IntegrationPointShouldBeSavedAndRetrievedProperly_WhenFieldMappingJsonIsLongerThan10000()
		{
			CreateFields(_sourceWorkspaceID);
			var field = RetrieveIdentifierField(_sourceWorkspaceID);
		}

		private void CreateFields(int workspaceID)
		{
			const int numberOfFields = 1000;
			const string fieldNamePrefix = "Very_Long_Field_Name_00000000000000000000000000";
			for (int i = 0; i < numberOfFields; i++)
			{
				var fixedLengthFieldRequest = new FixedLengthFieldRequest
				{
					ObjectType = new ObjectTypeIdentifier {ArtifactTypeID = (int) ArtifactType.Document},
					Name = $"{fieldNamePrefix}{i}",
					Length = 255,
					IsRequired = false,
					IncludeInTextIndex = false,
					HasUnicode = true,
					AllowHtml = false,
					OpenToAssociations = false
				};
				_fieldManager.CreateFixedLengthFieldAsync(workspaceID, fixedLengthFieldRequest).GetAwaiter().GetResult();
			}
		}

		private ArtifactDTO RetrieveIdentifierField(int workspaceID)
		{
			IFieldQueryRepository fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(workspaceID);
			return fieldQueryRepository.RetrieveTheIdentifierField((int) ArtifactType.Document);
		}
	}
}
