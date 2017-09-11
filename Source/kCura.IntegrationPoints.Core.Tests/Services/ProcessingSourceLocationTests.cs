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
    }
}
