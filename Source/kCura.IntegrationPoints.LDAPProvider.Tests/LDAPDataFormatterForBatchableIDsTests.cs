using NUnit.Framework;
using System;
using FluentAssertions;
using Relativity.API;

namespace kCura.IntegrationPoints.LDAPProvider.Tests
{
    [TestFixture, Category("Unit")]
    public class LDAPDataFormatterForBatchableIDsTests
    {
        private LDAPDataFormatterForBatchableIDs _formatter;

        [SetUp]
        public void CreateFormatter()
        {
            var settings = NSubstitute.Substitute.For<LDAPSettings>();
            var helper = NSubstitute.Substitute.For<IHelper>();
            _formatter = new LDAPDataFormatterForBatchableIDs(settings, helper);
        }

        [Test]
        [TestCase(new byte[] {2, 20}, "\\02\\14")]
        [TestCase(new byte[] { }, "")]
        public void ConvertValidByteArray(byte[] array, string expectedRepresentation)
        {
            // Act / Assert
            _formatter.ConvertByteArray(array).Should().Be(expectedRepresentation);
            _formatter.ConvertByteArray(array).Should().Be(expectedRepresentation);
        }

        [Test]
        public void ThrowExceptionOnNullArray()
        {
            // Act / Assert
            Assert.Throws<NullReferenceException>(() => _formatter.ConvertByteArray(null));
        }
    }
}
