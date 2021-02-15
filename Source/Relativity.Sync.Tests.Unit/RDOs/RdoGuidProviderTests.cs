using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Digests;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.Tests.System.Core;

namespace Relativity.Sync.Tests.Unit.RDOs
{
    [TestFixture]
    public class RdoGuidProviderTests
    {
        [Test]
        public void GetValue_ShouldReturnCorrectValue()
        {
            // Arrange 
            var sut = new RdoGuidProvider();
            
            // Act
            RdoTypeInfo value = sut.GetValue<SampleRdo>();
            
            // Assert
            var expectedValue = SampleRdo.ExpectedRdoInfo;
            
            value.Should().NotBeEquivalentTo(expectedValue);
        }
    }
}