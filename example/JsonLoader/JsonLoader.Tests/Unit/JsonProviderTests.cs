using System.Data;
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
			settings.FieldLocation = "fields.json";
			//ACT
			
			var fields = provider.GetFields(JsonConvert.SerializeObject(settings));

			System.IO.File.ReadAllBytes(@"C:\\Users\dbarnes\");

			//ASSERT
			Assert.AreEqual(5, fields.Count());
		}

		[Test]
		public void GetDataTests()
		{
			var provider = new JsonProvider(new JsonHelper());
			var result = provider.GetData(null, null, null);
			var dt = new DataTable();
			dt.Load(result);
		}

	}
}
