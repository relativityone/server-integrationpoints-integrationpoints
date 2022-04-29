using FluentAssertions;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Keplers
{
    public class ProviderManagerTests: TestsBase
    {
        private IProviderManager _sut;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _sut = Container.Resolve<IProviderManager>();
        }

        [IdentifiedTest("61397fae-fa20-4192-b8c6-845bccf01d0f")]
        [IdentifiedTestCase("61397fae-fa20-4192-b8c6-845bccf01d0f", SourceProviders.RELATIVITY)]
        [IdentifiedTestCase("d49f14dc-eed5-452e-b574-62353cf1583e", SourceProviders.FTP)]
        [IdentifiedTestCase("010f515f-286e-491c-8efb-221d4c7df0b5", SourceProviders.LDAP)]
        [IdentifiedTestCase("a548b672-bf83-4fe7-acf9-da3c6b836707", SourceProviders.IMPORTLOADFILE)]
        public async Task GetSourceProviderArtifactIdAsync_ShouldReturnCorrectValues(string sourceProviderGuidIdentifier)
        {
            //Arrange           

            //Act
            int result = await _sut.GetSourceProviderArtifactIdAsync(SourceWorkspace.ArtifactId, sourceProviderGuidIdentifier).ConfigureAwait(false);

            //Assert
            result.Should().Be(10);

        }

        [IdentifiedTestCase("61397fae-fa20-4192-b8c6-845bccf01d0f", DestinationProviders.LOADFILE)]
        [IdentifiedTestCase("d49f14dc-eed5-452e-b574-62353cf1583e", DestinationProviders.RELATIVITY)]
        public async Task GetDestinationProviderArtifactIdAsync_ShouldReturnCorrectValues(string destinationProviderGuidIdentifier)
        {
            //Arrange           

            //Act
            int result = await _sut.GetDestinationProviderArtifactIdAsync(SourceWorkspace.ArtifactId, destinationProviderGuidIdentifier).ConfigureAwait(false);

            //Assert
            result.Should().Be(10);

        }

        [IdentifiedTest("61397fae-fa20-4192-b8c6-845bccf01d0f")]
        public async Task GetSourceProviders_ShouldReturnCorrectValues()
        {
            //Arrange           

            //Act
            IList<ProviderModel> result = await _sut.GetSourceProviders(SourceWorkspace.ArtifactId).ConfigureAwait(false);

            //Assert

        }

        [IdentifiedTest("61397fae-fa20-4192-b8c6-845bccf01d0f")]
        public async Task GetDestinationProviders_ShouldReturnCorrectValues()
        {
            //Arrange           

            //Act
            IList<ProviderModel> result = await _sut.GetDestinationProviders(SourceWorkspace.ArtifactId).ConfigureAwait(false);

            //Assert

        }
    }
}
