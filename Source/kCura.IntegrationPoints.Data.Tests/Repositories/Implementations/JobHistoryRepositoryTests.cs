using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ExceptionTestService;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class JobHistoryRepositoryTests
    {
        private JobHistoryRepository _sut;
        private Mock<IRelativityObjectManager> _relativityObjectManagerMock;

        private static DateTime[] _jobEndDates =
        {
            new DateTime(2015, 5, 28, 10, 46, 32),
            DateTime.UtcNow,
            DateTime.MaxValue
        };
        private readonly int _integrationPointID = 210300;
        private readonly int _jobHistoryID = 123;
        private readonly DateTime _jobEndTime = new DateTime(2015, 5, 28, 10, 46, 33);

        [SetUp]
        public void SetUp()
        {
            _relativityObjectManagerMock = new Mock<IRelativityObjectManager>();
            _sut = new JobHistoryRepository(_relativityObjectManagerMock.Object);
        }

        [Test]
        [TestCaseSource("_jobEndDates")]
        public void MarkJobAsValidationFailed_ShouldCheckTheTime_WhenWeUpdateStatusField(DateTime jobEndDate)
        {
            //Act
            _sut.MarkJobAsValidationFailed(_jobHistoryID, _integrationPointID, jobEndDate);

            //Assert
            _relativityObjectManagerMock.Verify(x => x.Update(_jobHistoryID,
                It.Is<List<FieldRefValuePair>>(y => y.Any(z => z.Field.Guid == JobHistoryFieldGuids.EndTimeUTCGuid && (DateTime)z.Value == jobEndDate)), ExecutionIdentity.CurrentUser));
        }

        [Test]
        [TestCaseSource("_jobEndDates")]
        public void MarkJobAsFailed_ShouldCheckTheTime_WhenWeUpdateStatusField(DateTime jobEndDate)
        {
            //Act
            _sut.MarkJobAsFailed(_jobHistoryID, _integrationPointID, jobEndDate);

            //Assert
            _relativityObjectManagerMock.Verify(x => x.Update(_jobHistoryID,
                It.Is<List<FieldRefValuePair>>(y => y.Any(z => z.Field.Guid == JobHistoryFieldGuids.EndTimeUTCGuid && (DateTime)z.Value == jobEndDate)), ExecutionIdentity.CurrentUser));
        }

        [Test]
        public void MarkJobAsValidationFailed_ShouldThrowException_WhenUpdateStatusFails()
        {
            //Arrange
            _relativityObjectManagerMock.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<IList<FieldRefValuePair>>(),ExecutionIdentity.CurrentUser)).Throws<Exception>();

            //Act
            Action action = () =>_sut.MarkJobAsValidationFailed(_jobHistoryID, _integrationPointID, _jobEndTime);

            //Assert
            action.ShouldThrow<Exception>();
        }

        [Test]
        public void MarkJobAsFailed_ShouldThrowException_WhenUpdateStatusFails()
        {
            //Arrange
            _relativityObjectManagerMock.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<IList<FieldRefValuePair>>(), ExecutionIdentity.CurrentUser)).Throws<Exception>();

            //Act
            Action action = () => _sut.MarkJobAsFailed(_jobHistoryID, _integrationPointID, _jobEndTime);

            //Assert
            action.ShouldThrow<Exception>();
        }
    }
}