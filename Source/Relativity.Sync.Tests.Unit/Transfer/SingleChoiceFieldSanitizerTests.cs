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
	internal sealed class SingleChoiceFieldSanitizerTests
	{
		[Test]
		public void ItShouldSupportSingleChoice()
		{
			// Arrange
			var instance = new SingleChoiceFieldSanitizer();

			// Act
			RelativityDataType supportedType = instance.SupportedType;

			// Assert
			supportedType.Should().Be(RelativityDataType.SingleChoice);
		}

		[Test]
		public async Task ItShouldReturnNullValueUnchanged()
		{
			// Arrange
			var instance = new SingleChoiceFieldSanitizer();

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
			var instance = new SingleChoiceFieldSanitizer();

			// Act
			const int initialValue = 123123;
			Func<Task> action = async () => await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			(await action.Should().ThrowAsync<SyncException>().ConfigureAwait(false))
				.Which.Message.Should()
					.Contain(typeof(Choice).Name).And
					.Contain(typeof(int).Name);
		}

		[Test]
		public async Task ItShouldReturnChoiceName()
		{
			// Arrange
			var instance = new SingleChoiceFieldSanitizer();

			// Act
			const string expectedName = "Noice Choice";
			Choice initialValue = new Choice { Name = expectedName };
			object result = await instance.SanitizeAsync(0, "foo", "bar", "bang", initialValue).ConfigureAwait(false);

			// Assert
			result.Should().Be(expectedName);
		}
	}
}
