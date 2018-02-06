﻿using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	public class IntegrationPointTypeRepositoryTests : TestBase
	{
		private IntegrationPointTypeRepository _integrationPointTypeRepository;
		private IRelativityObjectManager _objectManager;

		public override void SetUp()
		{
			_objectManager = Substitute.For<IRelativityObjectManager>();
			var rsapiService = Substitute.For<IRSAPIService>();
			rsapiService.RelativityObjectManager.Returns(_objectManager);

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

			_objectManager.Query<IntegrationPointType>(Arg.Any<QueryRequest>()).Returns(expectedResult);

			var actualResult = _integrationPointTypeRepository.GetIntegrationPointTypes();

			Assert.That(actualResult,
				Is.EquivalentTo(expectedResult).Using(new Func<IntegrationPointTypeModel, IntegrationPointType, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactId))));
		}
	}
}