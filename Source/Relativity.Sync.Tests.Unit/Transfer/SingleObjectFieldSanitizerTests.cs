using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	internal class SingleObjectFieldSanitizerTests
	{
		[Test]
		public void ItShouldSupportSingleObject()
		{
			// Arrange
			var instance = new SingleObjectFieldSanitizer();

			// Act
			RelativityDataType supportedType = instance.SupportedType;

			// Assert
			supportedType.Should().Be(RelativityDataType.SingleObject);
		}

		[Test]
		public async Task ItShouldReturnNullValueUnchanged()
		{
			// Arrange
			var instance = new SingleObjectFieldSanitizer();

			// Act
			object initialValue = null;
			object result = await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			result.Should().BeNull();
		}

		[Test]
		public async Task ItShouldThrowSyncExceptionWhenEncounteringUnexpectedType()
		{
			// Arrange
			var instance = new SingleObjectFieldSanitizer();

			// Act
			const int initialValue = 1123123;
			Func<Task> action = async () => await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false))
				.Which.Message.Should()
				.Contain(typeof(RelativityObjectValue).Name).And
				.Contain(typeof(int).Name);
		}

		[Test]
		public async Task ItShouldReturnObjectName()
		{
			// Arrange
			var instance = new SingleObjectFieldSanitizer();

			// Act
			const string expectedName = "Awesome Object";
			RelativityObjectValue initialValue = new RelativityObjectValue { Name = expectedName };
			object result = await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			result.Should().Be(expectedName);
		}
	}
}
