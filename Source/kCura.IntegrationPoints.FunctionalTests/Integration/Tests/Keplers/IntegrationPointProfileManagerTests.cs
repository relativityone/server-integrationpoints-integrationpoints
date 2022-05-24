using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    public class IntegrationPointProfileManagerTests : TestsBase
    {
        private IIntegrationPointProfileManager _sut;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _sut = Container.Resolve<IIntegrationPointProfileManager>();
        }

        public async Task CreateIntegrationPointProfileAsync_ShouldReturnCorrectIntegrationPointModel()
        {
            //Arrange
            CreateIntegrationPointRequest request = new CreateIntegrationPointRequest();
            //Act
            IntegrationPointModel result = await _sut.CreateIntegrationPointProfileAsync(request);

            //Assert


        }
    }
}
