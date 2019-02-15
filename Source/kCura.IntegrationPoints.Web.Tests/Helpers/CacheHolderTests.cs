using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Web.Helpers;
using kCura.IntegrationPoints.Web.Logging;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Helpers
{
	public class CacheHolderTests : TestBase
	{
		private CacheHolder _cacheHolder;

		private static readonly string _key = "MyKey";

		public override void SetUp()
		{
			_cacheHolder = new CacheHolder();
		}

		[Test]
		public void ItShouldGetCachedObjects()
		{
			// Arrange
			string myValue = "Test";
			_cacheHolder.SetObject(_key, myValue);

			// Act
			string result = _cacheHolder.GetObject<string>(_key);

			// Assert
			Assert.That(result, Is.EqualTo(myValue));
		}

		[Test]
		public void ItShouldNotGetCachedObjects()
		{
			// Arrange
			string myValue = "ABC";
			_cacheHolder.SetObject(_key, myValue);

			// Act
			string result = _cacheHolder.GetObject<string>("");

			// Assert
			Assert.That(result, Is.Null);
		}
	}
}
