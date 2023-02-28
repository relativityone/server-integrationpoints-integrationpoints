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

        [IdentifiedTestCase("61397fae-fa20-4192-b8c6-845bccf01d0f", SourceProviders.RELATIVITY)]
        [IdentifiedTestCase("d49f14dc-eed5-452e-b574-62353cf1583e", SourceProviders.FTP)]
        [IdentifiedTestCase("010f515f-286e-491c-8efb-221d4c7df0b5", SourceProviders.LDAP)]
        [IdentifiedTestCase("a548b672-bf83-4fe7-acf9-da3c6b836707", SourceProviders.IMPORTLOADFILE)]
        public async Task GetSourceProviderArtifactIdAsync_ShouldReturnCorrectValues(string sourceProviderGuidIdentifier)
        {
            // Arrange
            Models.SourceProviderTest expectedArtifactId = SourceWorkspace.SourceProviders.Where(x => x.Identifier == sourceProviderGuidIdentifier).First();

            // Act
            int result = await _sut.GetSourceProviderArtifactIdAsync(SourceWorkspace.ArtifactId, sourceProviderGuidIdentifier).ConfigureAwait(false);

            // Assert
            result.Should().Be(expectedArtifactId.ArtifactId);

        }

        [IdentifiedTestCase("bb66cd69-c611-4874-affb-0e2ca7ba65ae", DestinationProviders.LOADFILE)]
        [IdentifiedTestCase("166f7a42-e511-4e49-ae14-361410afc4e8", DestinationProviders.RELATIVITY)]
        public async Task GetDestinationProviderArtifactIdAsync_ShouldReturnCorrectValues(string destinationProviderGuidIdentifier)
        {
            // Arrange
            Models.DestinationProviderTest expectedArtifactId = SourceWorkspace.DestinationProviders.Where(x => x.Identifier == destinationProviderGuidIdentifier).First();

            // Act
            int result = await _sut.GetDestinationProviderArtifactIdAsync(SourceWorkspace.ArtifactId, destinationProviderGuidIdentifier).ConfigureAwait(false);

            // Assert
            result.Should().Be(expectedArtifactId.ArtifactId);

        }

        [IdentifiedTest("5bd58099-643c-4244-948f-b630e13881f1")]
        public async Task GetSourceProviders_ShouldReturnCorrectValues()
        {
            // Arrange
            IList<ProviderModel> expected = new List<ProviderModel>();
            foreach (var x in SourceWorkspace.SourceProviders)
            {
                expected.Add(new ProviderModel
                {
                    ArtifactId = x.ArtifactId,
                    Name = x.Name
                });
            }

            // Act
            IList<ProviderModel> result = await _sut.GetSourceProviders(SourceWorkspace.ArtifactId).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expected.Count);
            result.ShouldAllBeEquivalentTo(expected);
        }

        [IdentifiedTest("877eedb3-84e1-45a2-bf19-88bc2a6dd4e5")]
        public async Task GetDestinationProviders_ShouldReturnCorrectValues()
        {
            // Arrange
            IList<ProviderModel> expected = new List<ProviderModel>();
            foreach (var x in SourceWorkspace.DestinationProviders)
            {
                expected.Add(new ProviderModel
                {
                    ArtifactId = x.ArtifactId,
                    Name = x.Name
                });
            }

            // Act
            IList<ProviderModel> result = await _sut.GetDestinationProviders(SourceWorkspace.ArtifactId).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expected.Count);
            result.ShouldAllBeEquivalentTo(expected);
        }
    }
}
