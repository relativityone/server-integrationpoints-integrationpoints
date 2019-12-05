using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
	[TestFixture]
	public class JobHistoryRepositoryTests
	{
		private JobHistoryRepository _sut;
		private Mock<IRelativityObjectManager> _relativityObjectManagerMock;
		private readonly int _integrationPointID = 210300;
		private readonly int _jobHistoryID = 123;
		private readonly DateTime _endTime = new DateTime(2015, 5, 28, 10, 46, 33);

		[SetUp]
		public void SetUp()
		{
			_relativityObjectManagerMock = new Mock<IRelativityObjectManager>();
			_sut = new JobHistoryRepository(_relativityObjectManagerMock.Object);
		}

		[Test]
		public void MarkJobAsValidationFailed_ShouldAssertDateTime()
		{
			//Act
			_sut.MarkJobAsValidationFailed(_jobHistoryID, _integrationPointID, () => _endTime);

			//Assert
			_relativityObjectManagerMock.Verify(x => x.Update(_jobHistoryID,
				It.Is<List<FieldRefValuePair>>(y => y.Any(z => z.Field.Guid == JobHistoryFieldGuids.EndTimeUTCGuid && (DateTime)z.Value == _endTime)),ExecutionIdentity.CurrentUser));
		}

		[Test]
		public void MarkJobAsFailed_ShouldAssertDateTime()
		{
			//Act
			_sut.MarkJobAsFailed(_jobHistoryID, _integrationPointID, () => _endTime);

			//Assert
			_relativityObjectManagerMock.Verify(x => x.Update(_jobHistoryID,
				It.Is<List<FieldRefValuePair>>(y => y.Any(z => z.Field.Guid == JobHistoryFieldGuids.EndTimeUTCGuid && (DateTime)z.Value == _endTime)), ExecutionIdentity.CurrentUser));
		}

	}
}