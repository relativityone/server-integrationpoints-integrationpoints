using NUnit.Framework;
using FluentAssertions;
using kCura.IntegrationPoints.Domain.Utils;

namespace kCura.IntegrationPoints.Domain.Tests
{
    [TestFixture, Category("Unit")]
    public class UtilsTests
    {
        [Test]
        [TestCase("name string", -4, "name string - -4")]
        [TestCase("name string", null, "name string")]
        [TestCase(null, 3, " - 3")]
        public void GetFormatForWorkspaceOrJobDisplayReturnsProperString(string name, int? id, string result)
        {
            // Act / Assert
            WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(name, id).Should().Be(result);
        }

        [Test]
        public void GetFormatForWorkspaceOrJobDisplayReturnsProperStringForPrefix()
        {
            // Act / Assert
            WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay("some prefix", "name string", -3)
                .Should()
                .Be("some prefix - name string - -3");
        }
    }
}
