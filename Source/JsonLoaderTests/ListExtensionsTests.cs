using System.Collections.Generic;
using JsonLoader;
using NUnit.Framework;

namespace JsonLoaderTests
{
	[TestFixture]
    public class ListExtensionsTests
    {
		List<DataObject> _objects = new List<DataObject>
		{
			new DataObject
			{
				ID0 = "1",
				ID1 = "a"
			},
			new DataObject
			{
				ID0 = "2",
				ID1 = "b"
			},
			new DataObject
			{
				ID0 = "3",
				ID1 = "c"
			}
		};

		[Test]
		public void Test()
		{
			_objects.ToBatchableIds("ID0");
		}

		[Test]
		public void Test2()
		{
			string identifier = "ID0";
			string[] fieldList = {"ID0", "ID1"};
			HashSet<string> entryIds = new HashSet<string> { "1", "2" };
			_objects.ToDataTable(identifier, fieldList, entryIds);
		}
	}
}
