using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class ProgressStateCounterTests
	{
		[Test]
		public void ItShouldIncrementOnEachCall()
		{
			const int numRounds = 10;
			var instance = new ProgressStateCounter();

			IEnumerable<int> expectedValues = Enumerable.Range(0, numRounds);
			List<int> actualValues = new List<int>();
			for (int i = 0; i < numRounds; i++)
			{
				int actual = instance.Next();
				actualValues.Add(actual);
			}

			CollectionAssert.AreEqual(expectedValues, actualValues);
		}

		[Test]
		public void ItShouldStartFromInitialValue()
		{
			const int numRounds = 100;
			Random rng = new Random();

			for (int i = 0; i < numRounds; i++)
			{
				int expected = rng.Next(Int32.MaxValue);
				int actual = new ProgressStateCounter(expected).Next();
				Assert.AreEqual(expected, actual);
			}
		}
	}
}
