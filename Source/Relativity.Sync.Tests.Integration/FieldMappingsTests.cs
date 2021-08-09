using System;
using System.Collections.Generic;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class FieldMappingsTests
	{
		private IFieldMappings _sut;

		[SetUp]
		public void SetUp()
		{
			const string fieldsMap = @"[{
		        ""sourceField"": {
		            ""displayName"": ""Control Number [Object Identifier]"",
		            ""isIdentifier"": true,
		            ""fieldIdentifier"": ""1003667"",
		            ""isRequired"": true
		        },
		        ""destinationField"": {
		            ""displayName"": ""Control Number [Object Identifier]"",
		            ""isIdentifier"": true,
		            ""fieldIdentifier"": ""1003668"",
		            ""isRequired"": true
		        },
		        ""fieldMapType"": ""Identifier""
		    }]";

			Mock<IConfiguration> configuration = new Mock<IConfiguration>();
			configuration.Setup(x => x.GetFieldValue(It.IsAny<Func<SyncConfigurationRdo, string>>())).Returns(fieldsMap);

			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			containerBuilder.RegisterInstance(configuration.Object).As<IConfiguration>();
			_sut = containerBuilder.Build().Resolve<IFieldMappings>();
		}

		[Test]
		public void ItShouldProperlyDeserializeFieldMappings()
		{
			// act
			IList<FieldMap> fieldMap = _sut.GetFieldMappings();

			// assert
			fieldMap.Should().NotBeNullOrEmpty();
			fieldMap.Count.Should().Be(1);
			fieldMap.Should().Contain(x => x.SourceField.DisplayName == "Control Number [Object Identifier]");
		}
	}
}