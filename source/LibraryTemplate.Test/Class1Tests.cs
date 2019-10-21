using System;
using NUnit.Framework;

namespace LibraryTemplate.Test
{
	[TestFixture]
	public class Class1Tests
	{
		[Test]
		[Category("Unit")]
		public void TestNameIsSet()
		{
			// Arrange
			Class1 test = new Class1("test_name");

			//Assert
			Assert.AreEqual("test_name", test.Name);
		}

		[Test]
		[Category("Unit")]
		public void TestNameIsUsed()
		{
			// Arrange
			Class1 test = new Class1("test_name");

			//Assert
			Assert.AreEqual("test_name", test.GetName());
		}
	}
}
