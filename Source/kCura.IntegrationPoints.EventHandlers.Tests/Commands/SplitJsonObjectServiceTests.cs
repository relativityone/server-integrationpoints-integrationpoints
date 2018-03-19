using System.Collections.Generic;
using kCura.IntegrationPoints.EventHandlers.Commands;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	public class SplitJsonObjectServiceTests
	{
		private const string sampleJson =
			"{\"debug_isOn\":\"on\",\"Window\":\"main_window\",\"image\":{\"src\":\"Images/Sun.png\",\"alignment\":\"center\"},\"pop_Up\":\"menu_item\"}";

		private const string emptyJson = @"{}";

		public static IEnumerable<TestCaseData> PositiveTestCases
		{
			get
			{
				yield return new TestCaseData(sampleJson, 
						new[] {"nonExistingProperty"},
						sampleJson,
						emptyJson
					).SetName("No properties are found");

				yield return new TestCaseData(sampleJson,
						new[] { "debug_isOn", "Window", "image", "pop_Up" },
						emptyJson,
						sampleJson
					).SetName("All properties were moved");

				yield return new TestCaseData(sampleJson,
						new[] { "Debug_IsOn", "Window", "Image" },
						"{\"debug_isOn\":\"on\",\"image\":{\"src\":\"Images/Sun.png\",\"alignment\":\"center\"},\"pop_Up\":\"menu_item\"}",
						"{\"Window\":\"main_window\"}"
					).SetName("Should be case sensitive");

				yield return new TestCaseData(sampleJson,
					new[] { "image", "debug_isOn" },
					"{\"Window\":\"main_window\",\"pop_Up\":\"menu_item\"}",
					"{\"image\":{\"src\":\"Images/Sun.png\",\"alignment\":\"center\"},\"debug_isOn\":\"on\"}"
				).SetName("Properties in different order");
			}
		}

		[TestCase(null)]
		[TestCase("")]
		[TestCase("notJson")]
		public void ShouldReturnNullWhenInvalidArgumentgIsPassed(string configurationString)
		{
			var service = new SplitJsonObjectService();

			SplittedJsonObject result = service.Split(configurationString);

			Assert.That(result, Is.Null);
		}

		[Test]
		[TestCaseSource(nameof(PositiveTestCases))]
		public void ShouldProperlySplitObject(string sourceJson, string[] propertiesToMove, string expectedSourceObject, string expectedObjectWithExtractedProperties)
		{
			var service = new SplitJsonObjectService();

			SplittedJsonObject result = service.Split(sourceJson, propertiesToMove);

			Assert.That(result.JsonWithoutExtractedProperties, Is.EqualTo(expectedSourceObject));
			Assert.That(result.JsonWithExtractedProperties, Is.EqualTo(expectedObjectWithExtractedProperties));
		}		
	}
}