using System;
using NUnit.Framework;

namespace LibraryTemplate.Test
{
	[TestFixture]
	public class Class2Tests
	{
		[Test]
		[Category("Unit")]
		public void TestClass2DoesntThrowIfNoNamer()
		{
			// Arrange
			Class2 tester = new Class2();

			//Assert
			Assert.AreEqual("I have no name.", tester.GetName());
		}

		[Test]
		[Category("Unit")]
		public void TestNameOfNamerIsUsed()
		{
			// Arrange
			Class1 test = new Class1("test_name");
			Class2 tester = new Class2(test);

			//Assert
			Assert.AreEqual("test_name", tester.GetName());
		}
	}
}
