using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.TestGuard
{
    [TestFixture]
    public class TestAttributesGuard
    {
        private List<Type> _testFixtures;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _testFixtures = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.GetCustomAttribute<TestFixtureAttribute>() != null)
                .Where(x => x.FullName != this.GetType().FullName)
                .ToList();
        }

        [Test]
        public void SystemTests_ShouldHaveProperFeatureAttributes()
        {
            // Arrange
            List<string> fixturesWithMissingFeatureAttribute = new List<string>();

            // Act
            foreach (Type testFixture in _testFixtures)
            {
                var seyncFeatureAttribute = testFixture.GetCustomAttribute<Feature.DataTransfer.IntegrationPoints.Sync>();
                if (seyncFeatureAttribute == null)
                {
                    fixturesWithMissingFeatureAttribute.Add(testFixture.FullName);
                }
            }

            // Assert
            fixturesWithMissingFeatureAttribute.Should().BeEmpty("all test fixtures should have proper feature Attribute");
        }

        [Test]
        public void SystemTests_ShouldHaveIdentifiedTestAttribute()
        {
            // Arrange
            List<string> testsWithMissingIdentifiedTestAttribute = new List<string>();

            // Act
            foreach (Type fixture in _testFixtures)
            {
                List<string> invalidTestMethods = fixture
                    .GetMethods()
                    .Where(x => x.GetCustomAttribute<TestAttribute>() != null && x.GetCustomAttribute<IdentifiedTestAttribute>() == null)
                    .Select(x => $"{fixture.FullName}.{x.Name}")
                    .ToList();

                testsWithMissingIdentifiedTestAttribute.AddRange(invalidTestMethods);
            }

            // Assert
            testsWithMissingIdentifiedTestAttribute.Should().BeEmpty("all tests should have IdentifiedTest attribute");
        }

        [Test]
        public void SystemTests_ShouldAllHaveUniqueGuids()
        {
            // Arrange
            Dictionary<string, List<string>> guidToMethodsDictionary = new Dictionary<string, List<string>>();

            // Act
            foreach (Type fixture in _testFixtures)
            {
                List<KeyValuePair<string, string>> guidAndMethodKeyValuePairs = fixture
                    .GetMethods()
                    .Where(x => x.GetCustomAttribute<IdentifiedTestAttribute>() != null)
                    .Select(x => new KeyValuePair<string, string>(x.GetCustomAttribute<IdentifiedTestAttribute>().Id, $"{fixture.FullName}.{x.Name}"))
                    .ToList();

                foreach (KeyValuePair<string, string> keyValuePair in guidAndMethodKeyValuePairs)
                {
                    if (guidToMethodsDictionary.ContainsKey(keyValuePair.Key))
                    {
                        guidToMethodsDictionary[keyValuePair.Key].Add(keyValuePair.Value);
                    }
                    else
                    {
                        guidToMethodsDictionary.Add(keyValuePair.Key, new List<string>()
                        {
                            keyValuePair.Value
                        });
                    }
                }
            }

            // Assert
            List<KeyValuePair<string, List<string>>> testsWithDuplicatedGuids = guidToMethodsDictionary.Where(x => x.Value.Count > 1).ToList();
            testsWithDuplicatedGuids.Count.Should().Be(0, $"all tests should have unique GUIDs, but the results are:{Environment.NewLine}{JsonConvert.SerializeObject(testsWithDuplicatedGuids, Formatting.Indented)}");
        }
    }
}