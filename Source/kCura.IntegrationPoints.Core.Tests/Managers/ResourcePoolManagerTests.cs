using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.RSAPIClient;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Workspace = kCura.Relativity.Client.DTOs.Workspace;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
	[TestFixture]
	public class ResourcePoolManagerTests : TestBase
	{
		private class ResourcePoolManagerTestImpl : ResourcePoolManager
		{
			public ResourcePoolManagerTestImpl(IRepositoryFactory repositoryFactory, IHelper helper, IRsapiClientFactory rsapiClientFactory)
				: base(repositoryFactory, helper, rsapiClientFactory)
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
		private IResourcePoolRepository _resourcePoolRepositoryMock;

		private const int _RESOURCE_POOL_ID = 1234;

		private readonly ProcessingSourceLocationDTO _processingSourceLocation = new ProcessingSourceLocationDTO()
		{
			ArtifactId = 1,
			Location = @"\\localhost\Export"
		};

		#endregion //Fields

		[SetUp]
		public override void SetUp()
		{
			_repositoryFactoryMock = Substitute.For<IRepositoryFactory>();
			_resourcePoolRepositoryMock = Substitute.For<IResourcePoolRepository>();
			var helper = Substitute.For<IHelper>();
			var rsapiClientFactory = Substitute.For<IRsapiClientFactory>();
			_repositoryFactoryMock.GetResourcePoolRepository().Returns(_resourcePoolRepositoryMock);

			_subjectUnderTest = new ResourcePoolManagerTestImpl(_repositoryFactoryMock, helper, rsapiClientFactory);
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
