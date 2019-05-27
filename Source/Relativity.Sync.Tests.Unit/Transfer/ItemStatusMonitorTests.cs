using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	public sealed class ItemStatusMonitorTests
	{
		private ItemStatusMonitor _instance;

		private const string _FIRST_ITEM_IDENTIFIER = "first";
		private const int _FIRST_ITEM_ARTIFACT_ID = 1;
		private const string _SECOND_ITEM_IDENTIFIER = "second";
		private const int _SECOND_ITEM_ARTIFACT_ID = 2;

		[SetUp]
		public void SetUp()
		{
			_instance = new ItemStatusMonitor();
		}

		[Test]
		public void ItShouldReturnNoSuccessfulItemsAfterAdding()
		{
			// Arrange
			_instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
			_instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);

			// Act
			IEnumerable<int> result = _instance.GetSuccessfulItemArtifactIds();

			// Assert
			result.Should().BeEmpty();
		}

		[Test]
		public void ItShouldReturnNoSuccessfulItemsAfterMarkingAsRead()
		{
			// Arrange
			_instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
			_instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
			_instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
			_instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);

			// Act
			IEnumerable<int> result = _instance.GetSuccessfulItemArtifactIds();

			// Assert
			result.Should().BeEmpty();
		}

		[Test]
		public void ItShouldReturnNoSuccessfulItemsAfterMarkingAllReadAsFailed()
		{
			// Arrange
			_instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
			_instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
			_instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
			_instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);
			_instance.MarkReadSoFarAsFailed();

			// Act
			IEnumerable<int> result = _instance.GetSuccessfulItemArtifactIds();

			// Assert
			result.Should().BeEmpty();
		}

		[Test]
		public void ItShouldReturnOneSuccessfulItemWhenOtherNotRead()
		{
			// Arrange
			const int successfulItemArtifactId = _FIRST_ITEM_ARTIFACT_ID;
			const int notReadItemArtifactId = _SECOND_ITEM_ARTIFACT_ID;

			_instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
			_instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
			_instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
			_instance.MarkReadSoFarAsSuccessful();

			// Act
			List<int> result = _instance.GetSuccessfulItemArtifactIds().ToList();

			// Assert
			result.Should().Contain(successfulItemArtifactId);
			result.Should().NotContain(notReadItemArtifactId);
		}

		[Test]
		public void ItShouldReturnSuccessfulItems()
		{
			// Arrange
			_instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
			_instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
			_instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
			_instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);
			_instance.MarkReadSoFarAsSuccessful();

			// Act
			List<int> result = _instance.GetSuccessfulItemArtifactIds().ToList();

			// Assert
			result.Should().Contain(_FIRST_ITEM_ARTIFACT_ID);
			result.Should().Contain(_SECOND_ITEM_ARTIFACT_ID);
		}

		[Test]
		public void ItShouldReturnSuccessfulItemWhenReadAfterMarkingAsFailed()
		{
			// Arrange
			const int successfulItemArtifactId = _SECOND_ITEM_ARTIFACT_ID;
			const int failedItemArtifactId = _FIRST_ITEM_ARTIFACT_ID;

			_instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
			_instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
			_instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
			_instance.MarkReadSoFarAsFailed();
			_instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);
			_instance.MarkReadSoFarAsSuccessful();

			// Act
			List<int> result = _instance.GetSuccessfulItemArtifactIds().ToList();

			// Assert
			result.Should().NotContain(failedItemArtifactId);
			result.Should().Contain(successfulItemArtifactId);
		}

		[Test]
		public void ItShouldReturnSuccessfulItemAfterMarkingOtherAsFailed()
		{
			// Arrange
			const int successfulItemArtifactId = _SECOND_ITEM_ARTIFACT_ID;
			const int failedItemArtifactId = _FIRST_ITEM_ARTIFACT_ID;

			_instance.AddItem(_FIRST_ITEM_IDENTIFIER, _FIRST_ITEM_ARTIFACT_ID);
			_instance.AddItem(_SECOND_ITEM_IDENTIFIER, _SECOND_ITEM_ARTIFACT_ID);
			_instance.MarkItemAsRead(_FIRST_ITEM_IDENTIFIER);
			_instance.MarkItemAsRead(_SECOND_ITEM_IDENTIFIER);
			_instance.MarkItemAsFailed(_FIRST_ITEM_IDENTIFIER);
			_instance.MarkReadSoFarAsSuccessful();

			// Act
			List<int> result = _instance.GetSuccessfulItemArtifactIds().ToList();

			// Assert
			result.Should().NotContain(failedItemArtifactId);
			result.Should().Contain(successfulItemArtifactId);
		}
	}
}
