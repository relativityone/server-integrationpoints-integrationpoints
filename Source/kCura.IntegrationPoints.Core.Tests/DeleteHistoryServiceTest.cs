﻿using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Tests
{
    [TestFixture, Category("Unit")]
    public class DeleteHistoryServiceTest : TestBase
    {
        private DeleteHistoryService _sut;
        private IIntegrationPointRepository _integrationPointRepository;

        [SetUp]
        public override void SetUp()
        {
	        _integrationPointRepository = Substitute.For<IIntegrationPointRepository>();

            _sut = new DeleteHistoryService(_integrationPointRepository);
        }

        [Test]
        public void ItShouldSetJobHistoryNull()
        {
            // arrange
            var integrationPointsIDs = new List<int> { 1, 2 };

            var integrationPoints = new List<Data.IntegrationPoint>
            {
                new Data.IntegrationPoint
                {
                    JobHistory = new[] {1, 2, 3}
                },
                new Data.IntegrationPoint
                {
                    JobHistory = new[] {1, 2, 3}
                }
            };

            _integrationPointRepository.GetAll(Arg.Any<List<int>>()).Returns(integrationPoints);

            // act
            _sut.DeleteHistoriesAssociatedWithIPs(integrationPointsIDs);

			// assert
			_integrationPointRepository.Received(2).Update(Arg.Is<Data.IntegrationPoint>(x => !x.JobHistory.Any()));
        }

        [Test]
        public void ItShouldNotQueryForIntegrationPointsWhenIDsListIsEmpty()
        {
            // arrange
            var integrationPointsIDs = new List<int>();

            // act
            _sut.DeleteHistoriesAssociatedWithIPs(integrationPointsIDs);

			// assert
			_integrationPointRepository
				.DidNotReceiveWithAnyArgs()
                .GetAll(Arg.Any<List<int>>());
        }

        [Test]
        public void ItShouldThrowExceptionWhenIDsListIsNull()
        {
            // act
            Action deleteAction = () => _sut.DeleteHistoriesAssociatedWithIPs(null);

            // assert
            deleteAction.Should().Throw<ArgumentNullException>("because input list was empty");
        }
    }
}