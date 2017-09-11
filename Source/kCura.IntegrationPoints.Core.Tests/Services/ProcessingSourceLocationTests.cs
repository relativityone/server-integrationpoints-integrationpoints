using System;
using System.Collections.Generic;
using System.IO;
using SystemInterface.IO;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Workspace = kCura.Relativity.Client.DTOs.Workspace;

namespace kCura.IntegrationPoints.Core.Tests.Services
{
    public class ProcessingSourceLocationServiceTests : TestBase
    {
        private const int _WORKSPACE_ID = 1;
        private const string _PROCESSING_SOURCE_LOCATION = @"\\PSL\CorrectPath";

        private ProcessingSourceLocationService _subjectUnderTest;

        private IResourcePoolContext _resourcePoolContextMock;
        private IResourcePoolManager _resourcePoolManagerMock;

        public override void SetUp()
        {
            _resourcePoolManagerMock = Substitute.For<IResourcePoolManager>();
            _resourcePoolContextMock = Substitute.For<IResourcePoolContext>();

            _subjectUnderTest = new ProcessingSourceLocationService(_resourcePoolContextMock,_resourcePoolManagerMock);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ItShouldCheckIfEnabled(bool enabled)
        {
            // Arrange
            _resourcePoolContextMock.IsProcessingSourceLocationEnabled().Returns(enabled);
            //Act
            bool result = _subjectUnderTest.IsEnabled();
            //Assert
            Assert.AreEqual(enabled,result);
        }


        [Test]
        [TestCase(false, @"\\PSL\CorrectPath")]
        [TestCase(true, @"\\PSL\CorrectPath")]
        [TestCase(false, @"\\PSL\IncorrectPath")]
        [TestCase(true, @"\\PSL\IncorrectPath")]
        public void ItShouldCheckIfPathIsProcessingSourceLocation(bool enabled, string path)
        {
            // Arrange
            _resourcePoolContextMock.IsProcessingSourceLocationEnabled().Returns(enabled);
            _resourcePoolManagerMock.GetProcessingSourceLocation(_WORKSPACE_ID)
                .Returns(new List<ProcessingSourceLocationDTO>()
                {
                    new ProcessingSourceLocationDTO() {Location = _PROCESSING_SOURCE_LOCATION}
                });

            
            //Act
            bool result = _subjectUnderTest.IsProcessingSourceLocation(path, _WORKSPACE_ID);
            bool expected = enabled && path == _PROCESSING_SOURCE_LOCATION;
            //Assert
            Assert.AreEqual(expected, result);
        }
    }
}
