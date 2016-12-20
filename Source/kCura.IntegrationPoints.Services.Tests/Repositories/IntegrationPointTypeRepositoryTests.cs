using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	public class IntegrationPointTypeRepositoryTests : TestBase
	{
		private IntegrationPointTypeRepository _integrationPointTypeRepository;
		private IGenericLibrary<IntegrationPointType> _library;

		public override void SetUp()
		{
			_library = Substitute.For<IGenericLibrary<IntegrationPointType>>();
			var rsapiService = Substitute.For<IRSAPIService>();
			rsapiService.IntegrationPointTypeLibrary.Returns(_library);

			_integrationPointTypeRepository = new IntegrationPointTypeRepository(rsapiService);
		}

		[Test]
		public void ItShouldRetrieveAllIntegrationPointTypes()
		{
			var expectedResult = new List<IntegrationPointType>
			{
				new IntegrationPointType
				{
					ArtifactId = 481,
					Name = "name_871"
				},
				new IntegrationPointType
				{
					ArtifactId = 377,
					Name = "name_454"
				}
			};

			var expectedQuery = new AllIntegrationPointTypesQueryBuilder().Create();

			_library.Query(Arg.Is<Query<RDO>>(x => x.IsEqualOnTypeAndNameAndFields(expectedQuery))).Returns(expectedResult);

			var actualResult = _integrationPointTypeRepository.GetIntegrationPointTypes();

			Assert.That(actualResult,
				Is.EquivalentTo(expectedResult).Using(new Func<IntegrationPointTypeModel, IntegrationPointType, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactId))));
		}
	}
}