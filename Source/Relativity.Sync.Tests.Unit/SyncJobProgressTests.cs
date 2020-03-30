﻿using System;
using FluentAssertions;
using NUnit.Framework;
using Moq;
using Relativity.Services.Exceptions;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Unit.Stubs;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class SyncJobProgressTests
	{
		private SyncJobParameters _jobParameters;
		private ProgressRepositoryStub _progressRepository;
		private IProgressStateCounter _counter;
		private SyncJobProgress _instance;

		[SetUp]
		public void SetUp()
		{
			const int syncConfigurationArtifactId = 1010000;
			const int sourceWorkspaceArtifactId = 1020000;
			_jobParameters = new SyncJobParameters(syncConfigurationArtifactId, sourceWorkspaceArtifactId, 1);
			_progressRepository = new ProgressRepositoryStub();
			_counter = new ProgressStateCounter();
			_instance = new SyncJobProgress(_jobParameters, _progressRepository, _counter, Mock.Of<ISyncLog>());
		}

		[Test]
		public void ItShouldCreateProgressWhenItDoesNotExists()
		{
			ProgressStub progress = new ProgressStub();
			_progressRepository.ForCreate.Add(progress);

			// ACT
			SyncJobState state = new SyncJobState("FooBar", string.Empty, SyncJobStatus.New, null, null);
			_instance.Report(state);

			// ASSERT
			progress.Name.Should().Be("FooBar");
			progress.Status.Should().Be(SyncJobStatus.New);
		}

		[Test]
		public void ItShouldUpdateProgressWhenItExists()
		{
			ProgressStub progress = new ProgressStub("FooBar");
			_progressRepository.ForQuery.Add(progress);
			 
			// ACT
			SyncJobState state = new SyncJobState("FooBar", string.Empty, SyncJobStatus.Failed, "A problem happened", new InvalidOperationException());
			_instance.Report(state);

			// ASSERT
			progress.Status.Should().Be(SyncJobStatus.Failed);
			progress.Message.Should().Be("A problem happened");
			progress.ActualException.Should().BeOfType<InvalidOperationException>();
		}

		[Test]
		public void ItShouldAssignCorrectOrderValuesToEachStep()
		{
			ProgressStub progress1 = new ProgressStub();
			ProgressStub progress2 = new ProgressStub();
			ProgressStub progress3 = new ProgressStub();
			_progressRepository.ForCreate.Add(progress1);
			_progressRepository.ForCreate.Add(progress2);
			_progressRepository.ForCreate.Add(progress3);

			// ACT
			SyncJobState state1 = SyncJobState.Start("FooBar1");
			SyncJobState state2 = SyncJobState.Start("FooBar2");
			SyncJobState state3 = SyncJobState.Start("FooBar3");
			_instance.Report(state1);
			_instance.Report(state2);
			_instance.Report(state3);

			// ASSERT
			progress2.Order.Should().BeGreaterThan(progress1.Order);
			progress3.Order.Should().BeGreaterThan(progress2.Order);
		}

		[Test]
		public void ItShouldNotThrowWhenProgressQueryThrows()
		{
			Mock<IProgressRepository> progressRepositoryMock = new Mock<IProgressRepository>();
			progressRepositoryMock.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
				.Throws<ServiceException>();

			// ACT
			SyncJobState state = SyncJobState.Start("FooBar");
			_instance = new SyncJobProgress(_jobParameters, progressRepositoryMock.Object, _counter, Mock.Of<ISyncLog>());
			_instance.Report(state);

			// ASSERT
			Assert.Pass();
		}

		[Test]
		public void ItShouldNotThrowWhenProgressCreationThrows()
		{
			Mock<IProgressRepository> progressRepositoryMock = new Mock<IProgressRepository>();
			progressRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<int>(),
				It.IsAny<int>(),
				It.IsAny<string>(),
				It.IsAny<int>(),
				It.IsAny<SyncJobStatus>())).Throws<ServiceException>();

			// ACT
			SyncJobState state = SyncJobState.Start("FooBar");
			_instance = new SyncJobProgress(_jobParameters, progressRepositoryMock.Object, _counter, Mock.Of<ISyncLog>());
			_instance.Report(state);

			// ASSERT
			Assert.Pass();
		}

		[Test]
		public void ItShouldNotThrowWhenProgressUpdateThrows()
		{
			Mock<IProgressRepository> progressRepositoryMock = new Mock<IProgressRepository>();
			Mock<IProgress> progress = new Mock<IProgress>();
			progressRepositoryMock.Setup(x => x.QueryAsync(It.IsAny<int>(),
				It.IsAny<int>(),
				It.IsAny<string>())).ReturnsAsync(progress.Object);
			progress.Setup(x => x.SetStatusAsync(It.IsAny<SyncJobStatus>())).Throws<ServiceException>();

			// ACT
			SyncJobState state = SyncJobState.Start("FooBar");
			_instance = new SyncJobProgress(_jobParameters, progressRepositoryMock.Object, _counter, Mock.Of<ISyncLog>());
			_instance.Report(state);

			// ASSERT
			Assert.Pass();
		}
	}
}
