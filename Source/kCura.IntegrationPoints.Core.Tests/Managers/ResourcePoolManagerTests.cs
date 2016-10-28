using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	public class ResourcePoolManagerTests
	{
		private class ResourcePoolManagerTestImpl : ResourcePoolManager
		{
			public ResourcePoolManagerTestImpl(IRepositoryFactory repositoryFactory, IHelper helper) 
				: base(repositoryFactory, helper)
			{
			}

			protected override Workspace GetWorkspace(int workspaceId)
			{
				return workspaceId <= 0 ? null : new Workspace(workspaceId)
				{
					ResourcePoolID = _RESOURCE_POOL_ID
				};
			}
		}

		#region Fields

		private ResourcePoolManagerTestImpl _subjectUnderTest;

		private IRepositoryFactory _repositoryFactoryMock;
		private IRSAPIClient _rsApiClientMock;
		private IResourcePoolRepository _resourcePoolRepositoryMock;

		private const int _RESOURCE_POOL_ID = 1234;

		private readonly ProcessingSourceLocationDTO _processingSourceLocation = new ProcessingSourceLocationDTO()
		{
			ArtifactId = 1,
			Location = @"\\localhost\Export"
		};

		#endregion //Fields

		[SetUp]
		public void Init()
		{
			_repositoryFactoryMock = Substitute.For<IRepositoryFactory>();
			_rsApiClientMock = Substitute.For<IRSAPIClient>();
			_resourcePoolRepositoryMock = Substitute.For<IResourcePoolRepository>();
			var helper = Substitute.For<IHelper>();

			_repositoryFactoryMock.GetResourcePoolRepository().Returns(_resourcePoolRepositoryMock);

			_subjectUnderTest = new ResourcePoolManagerTestImpl(_repositoryFactoryMock, helper);
		}

		[Test]
		public void ItShouldReturnProcessingSourceLocations()
		{
			var procSourceLocations = new List<ProcessingSourceLocationDTO> { _processingSourceLocation };
			_resourcePoolRepositoryMock.GetProcessingSourceLocationsByResourcePool(_RESOURCE_POOL_ID).Returns(procSourceLocations);

			const int wkspId = 1;
			List<ProcessingSourceLocationDTO> processingSourceLocations = _subjectUnderTest.GetProcessingSourceLocation(wkspId);

			Assert.That(processingSourceLocations, Is.Not.Null);
			Assert.That(processingSourceLocations.Count, Is.EqualTo(1));
			Assert.That(processingSourceLocations[0], Is.EqualTo(_processingSourceLocation));
		}

		[Test]
		public void ItShouldThrowException()
		{
			const int wkspId = 0;
			Assert.Throws<ArgumentException>(() => _subjectUnderTest.GetProcessingSourceLocation(wkspId));

			_resourcePoolRepositoryMock.DidNotReceive().GetProcessingSourceLocationsByResourcePool(Arg.Any<int>());
		}
	}
}
