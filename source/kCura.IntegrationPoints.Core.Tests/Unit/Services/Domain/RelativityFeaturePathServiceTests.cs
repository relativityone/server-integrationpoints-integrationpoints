using System;
using System.IO;
using System.Reflection;
using kCura.IntegrationPoints.Core.Domain;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Services.Domain
{
	[TestFixture]
	public class RelativityFeaturePathServiceTests
	{

		[Test]
		public void GetFeaturePathsValue_Relativity91AndPrior_CorrectValues()
		{
			//ARRANGE
			//ACT
			MockService mockService = getDefaultMockService();
			mockService.NewRegistryStructure = false;

			//ASSERT
			Assert.AreEqual("AgentPath", mockService.AgentPath);
			Assert.AreEqual("WebPath", mockService.EddsPath);
			Assert.AreEqual("LibraryPath", mockService.LibraryPath);
			Assert.AreEqual("WebProcessingPath", mockService.WebProcessingPath);
		}

		[Test]
		public void GetFeaturePathsValue_Relativity91AndPriorOnDevEnvironment_ExpectedException()
		{
			//ARRANGE
			//ACT
			MockService mockService = NSubstitute.Substitute.For<MockService>();
			mockService.NewRegistryStructure = false;
			mockService.LibraryPath = null;
			mockService.GetFeaturePathsValueOverride(Arg.Any<string>()).Returns(string.Empty);

			//ASSERT
			Assert.Throws<Exception>(() => { string path = mockService.LibraryPath; }, "Could not retrieve LibraryPath.");
		}

		[Test]
		public void GetFeaturePathsValue_Relativity92AndUp_CorrectValues()
		{
			//ARRANGE
			//ACT
			RelativityFeaturePathService mockService = new RelativityFeaturePathService();
			mockService.NewRegistryStructure = true;
			BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;//| BindingFlags.GetProperty | BindingFlags.SetProperty;
			mockService.GetType().GetField("_baseInstallDir", bindFlags).SetValue(mockService, "XXX");

			//ASSERT
			Assert.AreEqual("XXX\\Agents", mockService.AgentPath);
			Assert.AreEqual("XXX\\EDDS", mockService.EddsPath);
			Assert.AreEqual("XXX\\Library", mockService.LibraryPath);
			Assert.AreEqual("XXX\\WebProcessing", mockService.WebProcessingPath);
		}

		[Test]
		public void GetFeaturePathsValue_CallNewRegistryStructureTwice_ValueRetrievedOnce()
		{
		}

		[Test]
		public void GetFeaturePathsValue_CallWebProcessingPathTwice_ValueRetrievedOnce()
		{
		}

		[Test]
		public void GetFeaturePathsValue_CallEddsPathTwice_ValueRetrievedOnce()
		{
		}

		[Test]
		public void GetFeaturePathsValue_CallAgentPathTwice_ValueRetrievedOnce()
		{
		}

		[Test]
		public void GetFeaturePathsValue_CallLibraryPathTwice_ValueRetrievedOnce()
		{
		}

		private MockService getDefaultMockService()
		{
			return new MockService()
			{
				DevEnvironmentLibPath = Path.Combine("c:\\", Guid.NewGuid().ToString())
			};
		}
	}

	public class MockService : RelativityFeaturePathService
	{
		public MockService()
		{

		}
		public string DevEnvironmentLibPath { get; set; }

		public virtual string GetFeaturePathsValueOverride(string keyName)
		{
			return keyName;
		}

		protected override string GetFeaturePathsValue(string keyName)
		{
			return GetFeaturePathsValueOverride(keyName);
		}

		protected override string GetDevEnvironmentLibPath()
		{
			return DevEnvironmentLibPath;
		}
	}
}
