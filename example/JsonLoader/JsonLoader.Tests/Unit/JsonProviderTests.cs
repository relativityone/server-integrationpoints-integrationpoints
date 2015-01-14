using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace JsonLoader.Tests.Unit
{
	[TestFixture]
	public class JsonProviderTests
	{
		[Test]
		public void GetFields_GetsCorrectValues()
		{
			//ARRANGE
			var provider = new JsonLoader.JsonProvider(new JsonHelper());
			var settings = new JsonSettings();
			settings.FileName = "fields.json";
			//ACT
			
			var fields = provider.GetFields(JsonConvert.SerializeObject(settings));

			System.IO.File.ReadAllBytes(@"");

			//ASSERT
			Assert.AreEqual(5, fields.Count());
		}
	}
}
