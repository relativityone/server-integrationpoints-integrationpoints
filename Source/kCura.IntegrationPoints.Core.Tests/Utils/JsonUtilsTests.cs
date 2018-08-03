using kCura.IntegrationPoints.Core.Utils;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Utils
{
	[TestFixture]
	public class JsonUtilsTests
	{
		private const string _OLD_PROPERTY_NAME = "Old";
		private const string _NEW_PROPERTY_NAME = "New";

		[Test]
		public void ItShouldReturnOriginalJson_IfPropertyIsNotPresent()
		{
			// arrange
			string input = "{\"PropertyName\":\"value\",\"Second\":123}";

			// act
			string actual = JsonUtils.ReplacePropertyNameIfPresent(input, _OLD_PROPERTY_NAME, _NEW_PROPERTY_NAME);

			// assert
			Assert.AreSame(input, actual);
		}

		[Test]
		public void ItShouldReplace_PropertyName_WhenPresent_AndValueIsString()
		{
			// arrange
			string input = $"{{\"PropertyName\":\"value\",\"{_OLD_PROPERTY_NAME}\":\"ExpectedValue\",\"Second\":123}}";

			// act
			string actual = JsonUtils.ReplacePropertyNameIfPresent(input, _OLD_PROPERTY_NAME, _NEW_PROPERTY_NAME);

			// assert
			string expected = $"{{\"PropertyName\":\"value\",\"{_NEW_PROPERTY_NAME}\":\"ExpectedValue\",\"Second\":123}}";
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ItShouldReplace_PropertyName_WhenPresent_AndValueIsNumber()
		{
			// arrange
			string input = $"{{\"PropertyName\":\"value\",\"{_OLD_PROPERTY_NAME}\":543,\"Second\":123}}";

			// act
			string actual = JsonUtils.ReplacePropertyNameIfPresent(input, _OLD_PROPERTY_NAME, _NEW_PROPERTY_NAME);

			// assert
			string expected = $"{{\"PropertyName\":\"value\",\"{_NEW_PROPERTY_NAME}\":543,\"Second\":123}}";
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ItShouldReplace_PropertyName_WhenPresent_AndValueIsArray()
		{
			// arrange
			string input = $"{{\"PropertyName\":\"value\",\"{_OLD_PROPERTY_NAME}\":[543,454],\"Second\":123}}";

			// act
			string actual = JsonUtils.ReplacePropertyNameIfPresent(input, _OLD_PROPERTY_NAME, _NEW_PROPERTY_NAME);

			// assert
			string expected = $"{{\"PropertyName\":\"value\",\"{_NEW_PROPERTY_NAME}\":[543,454],\"Second\":123}}";
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ItShouldReplace_PropertyName_WhenPresent_AndValueIsObject()
		{
			// arrange
			string input = $"{{\"PropertyName\":\"value\",\"{_OLD_PROPERTY_NAME}\":{{\"A\":1,\"B\":2}},\"Second\":123}}";

			// act
			string actual = JsonUtils.ReplacePropertyNameIfPresent(input, _OLD_PROPERTY_NAME, _NEW_PROPERTY_NAME);

			// assert
			string expected = $"{{\"PropertyName\":\"value\",\"{_NEW_PROPERTY_NAME}\":{{\"A\":1,\"B\":2}},\"Second\":123}}";
			Assert.AreEqual(expected, actual);
		}
	}
}
