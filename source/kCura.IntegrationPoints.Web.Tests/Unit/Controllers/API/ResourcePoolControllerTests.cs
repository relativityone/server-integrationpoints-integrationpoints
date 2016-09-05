﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.Relativity.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Unit.Controllers
{
	public class ResourcePoolControllerTests
	{
		#region Fields

		private ResourcePoolController _subjectUnderTest;

		private IResourcePoolManager _resourcePoolManagerMock;
		private IDirectoryTreeCreator _directoryTreeCreatorMock;
		private IRepositoryFactory _repositoryFactoryMock;
		private IErrorRepository _errorRepositoryMock;

		private const int _WORKSPACE_ID = 1;
		private const int _PROC_SOURCE_LOC_ID = 2;

		private readonly ProcessingSourceLocationDTO _processingSourceLocation = new ProcessingSourceLocationDTO
		{
			ArtifactId = _PROC_SOURCE_LOC_ID,
			Location = @"\\localhost\Export"
		};

		#endregion //Fields

		[SetUp]
		public void Init()
		{
			_resourcePoolManagerMock = Substitute.For<IResourcePoolManager>();
			_directoryTreeCreatorMock = Substitute.For<IDirectoryTreeCreator>();
			_repositoryFactoryMock = Substitute.For<IRepositoryFactory>();
			_errorRepositoryMock = Substitute.For<IErrorRepository>();

			_repositoryFactoryMock.GetErrorRepository().Returns(_errorRepositoryMock);

			_subjectUnderTest = new ResourcePoolController(_resourcePoolManagerMock, _directoryTreeCreatorMock, 
				_repositoryFactoryMock);

			_subjectUnderTest.Request = new HttpRequestMessage();
			_subjectUnderTest.Request.SetConfiguration(new HttpConfiguration());
		}

		[Test]
		public void ItShouldGetProcessingSourceLocations()
		{
			// Arrange
			var procSourceLocations = new List<ProcessingSourceLocationDTO> { _processingSourceLocation };

			_resourcePoolManagerMock.GetProcessingSourceLocation(_WORKSPACE_ID).Returns(procSourceLocations);

			// Act
			HttpResponseMessage httpResponseMessage = _subjectUnderTest.GetProcessingSourceLocations(_WORKSPACE_ID);

			// Assert
			List<ProcessingSourceLocationDTO> retValue;
			httpResponseMessage.TryGetContentValue(out retValue);

			Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(retValue.Count, Is.EqualTo(1));
			Assert.That(retValue[0], Is.EqualTo(_processingSourceLocation));
		}

		[Test]
		public void ItShouldHandleExceptionOnGetProcessingSourceLocations()
		{
			// Arrange
			_resourcePoolManagerMock.GetProcessingSourceLocation(_WORKSPACE_ID).Throws<Exception>();

			// Act
			HttpResponseMessage httpResponseMessage = _subjectUnderTest.GetProcessingSourceLocations(_WORKSPACE_ID);

			// Assert
			AssertInternalErrorCode(httpResponseMessage);
		}

		[Test]
		public void ItShouldGetProcessingSourceLocationStructure()
		{
			// Arrange
			DirectoryTreeItem directoryTreeItem = new DirectoryTreeItem()
			{
				Id = "A",
				Text = "B"
			};

			var procSourceLocations = new List<ProcessingSourceLocationDTO> { _processingSourceLocation };

			_resourcePoolManagerMock.GetProcessingSourceLocation(_WORKSPACE_ID).Returns(procSourceLocations);
			_directoryTreeCreatorMock.TraverseTree(_processingSourceLocation.Location).Returns(directoryTreeItem);

			// Act
			HttpResponseMessage httpResponseMessage = _subjectUnderTest.GetProcessingSourceLocationStructure(_WORKSPACE_ID, _PROC_SOURCE_LOC_ID);

			// Assert
			DirectoryTreeItem retValue;
			httpResponseMessage.TryGetContentValue(out retValue);

			Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			Assert.That(retValue, Is.EqualTo(directoryTreeItem));
		}

		[Test]
		public void ItShouldHandleExceptionOnGetFolderStructure()
		{
			// Arrange
			_resourcePoolManagerMock.GetProcessingSourceLocation(_WORKSPACE_ID).Throws<Exception>();

			// Act
			HttpResponseMessage httpResponseMessage = _subjectUnderTest.GetProcessingSourceLocationStructure(_WORKSPACE_ID, _PROC_SOURCE_LOC_ID);

			// Assert
			AssertInternalErrorCode(httpResponseMessage);
		}

		[Test]
		public void ItShouldNotFoundProcessingSourceLocation()
		{
			// Arrange
			_resourcePoolManagerMock.GetProcessingSourceLocation(_WORKSPACE_ID).Returns(new List<ProcessingSourceLocationDTO>());

			// Act
			HttpResponseMessage httpResponseMessage = _subjectUnderTest.GetProcessingSourceLocationStructure(_WORKSPACE_ID, _PROC_SOURCE_LOC_ID);

			// Assert
			Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
		}

		private void AssertInternalErrorCode(HttpResponseMessage httpResponseMessage)
		{
			Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
			_errorRepositoryMock.Received().Create(Arg.Any<ErrorDTO[]>());
		}
	}
}
