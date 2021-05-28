using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Import;
using NUnit.Framework;
using Moq;
using System;
using kCura.ScheduleQueue.Core.Core;
using FluentAssertions;

namespace kCura.IntegrationPoints.Core.Contracts.Tests.Agent
{
	[TestFixture, Category("Unit")]
	public class TaskParametersBuilderTests
	{
		private TaskParametersBuilder _sut;

		private const long _LOAD_FILE_SIZE = 1000;
		private readonly DateTime _LOAD_FILE_MODIFIED_DATE = new DateTime(2020, 1, 1);

		private readonly Guid _BATCH_INSTANCE_ID = Guid.NewGuid();

		[SetUp]
		public void Setup()
		{
			Mock<IImportFileLocationService> importFileLocationService = new Mock<IImportFileLocationService>();
			importFileLocationService.Setup(x => x.LoadFileInfo(It.IsAny<Data.IntegrationPoint>()))
				.Returns(new Data.LoadFileInfo
				{
					Size = _LOAD_FILE_SIZE,
					LastModifiedDate = _LOAD_FILE_MODIFIED_DATE
				});

			_sut = new TaskParametersBuilder(
				importFileLocationService.Object);
		}

		[Test]
		public void Build_ShouldReturnLoadFileInfo_WhenImportLoadFileTaskTypeIsSelected()
		{
			// Act
			TaskParameters taskParameters = _sut.Build(TaskType.ImportService, _BATCH_INSTANCE_ID, It.IsAny<Data.IntegrationPoint>());

			// Assert
			taskParameters.BatchInstance.Should().Be(_BATCH_INSTANCE_ID);
			
			taskParameters.BatchParameters.Should().BeOfType<LoadFileTaskParameters>();

			LoadFileTaskParameters loadFileTaskParameters = taskParameters.BatchParameters as LoadFileTaskParameters;
			loadFileTaskParameters.LastModifiedDate.Should().Be(_LOAD_FILE_MODIFIED_DATE);
			loadFileTaskParameters.Size.Should().Be(_LOAD_FILE_SIZE);
		}

		[Test]
		public void Build_ShouldReturnEmptyTaskParameters_WhenAnyTaskTypeWasSelected()
		{
			// Act
			TaskParameters taskParameters = _sut.Build(It.IsAny<TaskType>(), _BATCH_INSTANCE_ID, It.IsAny<Data.IntegrationPoint>());

			// Assert
			taskParameters.BatchInstance.Should().Be(_BATCH_INSTANCE_ID);

			taskParameters.BatchParameters.Should().BeNull();
		}
	}
}
