using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Helpers
{
	[TestFixture]
	public class UrlVersionDecoratorTests
	{
		private string _assemblyVersion;

		[SetUp]
		public void Setup()
		{
			_assemblyVersion = Assembly.GetAssembly(typeof(UrlVersionDecorator)).GetName().Version.ToString(4);
		}

		[Test]
		public void AppendVersion_ShouldAddAmpersand_WhenThereAreParameters()
		{
			// Arrange
			const string url = "someUrl.aspx?param=x";

			// Act
			string result = UrlVersionDecorator.AppendVersion(url);


			// Assert
			result.EndsWith("&v=" + _assemblyVersion).Should().BeTrue();
		}

		[Test]
		public void AppendVersion_ShouldAddQuestionMark_WhenThereAreNoParameters()
		{
			// Arrange
			const string url = "someUrl.aspx";

			// Act
			string result = UrlVersionDecorator.AppendVersion(url);


			// Assert
			result.EndsWith("?v=" + _assemblyVersion).Should().BeTrue();
		}
	}
}
