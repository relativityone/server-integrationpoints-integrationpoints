using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    public class IntegrationPointTypeManagerTests : TestsBase
    {
        private IIntegrationPointTypeManager _sut;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _sut = Container.Resolve<IIntegrationPointTypeManager>();
        }

        [IdentifiedTest("DE4B8F92-5BC8-4DAD-920F-AC120C9303A3")]
        public async Task GetIntegrationPointTypes_ShouldReturnCorrectValues()
        {
            //Arrange           
            IList<IntegrationPointTypeTest> expectedIntegrationPointTypes = SourceWorkspace.IntegrationPointTypes;

            //Act
            IList<IntegrationPointTypeModel> result = await _sut.GetIntegrationPointTypes(SourceWorkspace.ArtifactId);

            //Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(expectedIntegrationPointTypes.Count());
            result.ShouldAllBeEquivalentTo(expectedIntegrationPointTypes);            
        }
    }
}
