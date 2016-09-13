﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Validators;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Unit.IntegrationPoints.Validators
{
	class PreCascadeDeleteEventHandlerValidatorTests
	{
		#region Fields

		private IPreCascadeDeleteEventHandlerValidator _subjectUnderTest;

		private IQueueManager _queueManagerMock;
		private IRepositoryFactory _repositoryFactoryMock;

		const int _INTEGRTION_POINT_ID = 1;
		const int _WKSP_ID = 2;

		#endregion //Fields

		[SetUp]
		public void Init()
		{
			_queueManagerMock = Substitute.For<IQueueManager>();
			_repositoryFactoryMock = Substitute.For<IRepositoryFactory>();

			_subjectUnderTest = new PreCascadeDeleteEventHandlerValidator(_queueManagerMock, _repositoryFactoryMock);
		}

		#region Tests

		[Test]
		public void ItShouldPassValidation()
		{
			// Arrange
			_queueManagerMock.HasJobsExecutingOrInQueue(_WKSP_ID, _INTEGRTION_POINT_ID).Returns(false);

			// Act
			_subjectUnderTest.Validate(_WKSP_ID, _INTEGRTION_POINT_ID);

			// Assert
			_repositoryFactoryMock.DidNotReceive().GetIntegrationPointRepository(_WKSP_ID);
		}

		[Test]
		public void ItShouldFailValidation()
		{
			// Arrange
			_queueManagerMock.HasJobsExecutingOrInQueue(_WKSP_ID, _INTEGRTION_POINT_ID).Returns(true);

			IIntegrationPointRepository integrationPointRepositoryMock = Substitute.For<IIntegrationPointRepository>();

			integrationPointRepositoryMock.Read(_INTEGRTION_POINT_ID).Returns(new IntegrationPointDTO());

			_repositoryFactoryMock.GetIntegrationPointRepository(_WKSP_ID).Returns(integrationPointRepositoryMock);

			// Act & Assert
			Assert.Throws<Exception>( () => _subjectUnderTest.Validate(_WKSP_ID, _INTEGRTION_POINT_ID));
		}

		#endregion //Tests
	}
}
