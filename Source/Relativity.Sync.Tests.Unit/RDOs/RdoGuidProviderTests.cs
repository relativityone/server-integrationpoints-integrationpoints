using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Digests;
using Polly;
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
            
            value.Name.Should().Be(expectedValue.Name);
            value.TypeGuid.Should().Be(expectedValue.TypeGuid);
            value.ParentTypeGuid.Should().Be(expectedValue.ParentTypeGuid);
            
            foreach (var fieldInfoKeyValue in value.Fields)
            {
                var expectedField = expectedValue.Fields[fieldInfoKeyValue.Key];
            
                fieldInfoKeyValue.Value.Guid.Should().Be(expectedField.Guid);
                fieldInfoKeyValue.Value.Name.Should().Be(expectedField.Name);
                fieldInfoKeyValue.Value.Type.Should().Be(expectedField.Type);
                fieldInfoKeyValue.Value.IsRequired.Should().Be(expectedField.IsRequired);
                fieldInfoKeyValue.Value.TextLenght.Should().Be(expectedField.TextLenght);
                fieldInfoKeyValue.Value.PropertyInfo.Should().BeSameAs(expectedField.PropertyInfo);
            }
        }
    }
}