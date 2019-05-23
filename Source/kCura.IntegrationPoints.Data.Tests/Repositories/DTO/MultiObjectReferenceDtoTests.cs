using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.DTO
{
	[TestFixture]
	public class MultiObjectReferenceDtoTest
	{
		[Test]
		public void ObjectReferences_ShouldReturnArrayWhenSingleItemConstructorWasUsed()
		{
			// arrange
			const int identifier = 4;
			var sut = new MultiObjectReferenceDto(identifier);
			int[] expectedResult = { identifier };

			// act
			IReadOnlyCollection<int> result = sut.ObjectReferences;

			// assert
			result.Should().BeEquivalentTo(expectedResult);
		}

		[TestCase(new int[0])]
		[TestCase(new[] { 4 })]
		[TestCase(new[] { -1, 5 })]
		[TestCase(new[] { 1, 2, 3, 4, 5 })]
		public void ObjectReferences_ShouldReturnArrayWhenCollectionConstructorWasUsed(IReadOnlyCollection<int> identifiers)
		{
			// arrange 
			var sut = new MultiObjectReferenceDto(identifiers);

			// act
			IReadOnlyCollection<int> result = sut.ObjectReferences;

			// assert
			result.Should().BeEquivalentTo(identifiers);
		}

		[Test]
		public void Value_ShouldReturnArrayWhenSingleItemConstructorWasUsed()
		{
			// arrange
			const int identifier = 4;
			var sut = new MultiObjectReferenceDto(identifier);
			int[] expectedResult = { identifier };

			// act
			object result = sut.Value;

			// assert
			result.Should().BeAssignableTo<int[]>();
			var resultAsArray = result as int[];
			resultAsArray.Should().BeEquivalentTo(expectedResult);
		}

		[TestCase(new int[0])]
		[TestCase(new[] { 4 })]
		[TestCase(new[] { -1, 5 })]
		[TestCase(new[] { 1, 2, 3, 4, 5 })]
		public void Value_ShouldReturnArrayWhenCollectionConstructorWasUsed(IReadOnlyCollection<int> identifiers)
		{
			// arrange 
			var sut = new MultiObjectReferenceDto(identifiers);

			// act
			object result = sut.Value;

			// assert
			result.Should().BeAssignableTo<int[]>();
			var resultAsArray = result as int[];
			resultAsArray.Should().BeEquivalentTo(identifiers);
		}

		[Test]
		public void ValueModificationShouldNotModifyObjectState()
		{
			// arrange
			IReadOnlyCollection<int> identifiers = new[] { 5, 9, 2 };
			int[] identifiersArray = identifiers.ToArray();

			var sut = new MultiObjectReferenceDto(identifiersArray);

			int[] valueArrayToModify = sut.Value as int[];

			// act
			valueArrayToModify[0] = 1;
			valueArrayToModify[1] = 2;
			valueArrayToModify[2] = 3;

			// assert
			identifiersArray.Should().BeEquivalentTo(identifiers);
			sut.ObjectReferences.Should().BeEquivalentTo(identifiers);
			int[] valueAsArray = sut.Value as int[];
			valueAsArray.Should().BeEquivalentTo(identifiers);
		}
	}
}
