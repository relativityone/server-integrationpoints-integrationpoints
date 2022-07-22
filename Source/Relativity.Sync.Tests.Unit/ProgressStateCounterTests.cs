using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public sealed class ProgressStateCounterTests
    {
        private ProgressStateCounter _instance;

        [SetUp]
        public void SetUp()
        {
            _instance = new ProgressStateCounter();
        }

        [Test]
        public void ItShouldIncrementOnEachCallToNext()
        {
            const int numRounds = 10;

            IEnumerable<int> expectedValues = Enumerable.Range(0, numRounds);
            List<int> actualValues = new List<int>();
            for (int i = 0; i < numRounds; i++)
            {
                int actual = _instance.Next();
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

        [TestCase(null)]
        [TestCase("")]
        public void ItShouldIncrementForNotSpecifiedGroupName(string groupName)
        {
            // act
            int value1 = _instance.GetOrderForGroup(groupName);
            int value2 = _instance.GetOrderForGroup(groupName);

            // assert
            Assert.AreNotEqual(value1, value2);
        }

        [Test]
        public void ItShouldReturnSameValueForGroup()
        {
            const string groupName = "Some group";

            // act
            int value1 = _instance.GetOrderForGroup(groupName);
            int value2 = _instance.GetOrderForGroup(groupName);

            // assert
            Assert.AreEqual(value1, value2);
        }
    }
}
