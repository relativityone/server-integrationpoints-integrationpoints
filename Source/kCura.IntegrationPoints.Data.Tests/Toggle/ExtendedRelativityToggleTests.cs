using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data.Toggle;
using NSubstitute;
using NUnit.Framework;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Data.Tests.Toggle
{
	[TestFixture]
	public class ExtendedRelativityToggleTests
	{
		private ExtendedRelativityToggle _toggle;
		private IToggleProvider _baseToggle;

		[SetUp]
		public void Setup()
		{
			_baseToggle = NSubstitute.Substitute.For<IToggleProvider>();
			_toggle = new ExtendedRelativityToggle(_baseToggle) {Configuration = NSubstitute.Substitute.For<IConfig>()};
		}

		[Test]
		public void IsAOAGFeatureEnabled_IsCloudInstance()
		{
			// arrange
			_toggle.Configuration.IsCloudInstance.Returns(true);
			_toggle.Configuration.UseEDDSResource.Returns(false);

			// act
			bool result = _toggle.IsAOAGFeatureEnabled();

			// assert 
			Assert.IsTrue(result);
		}

		[Test]
		public void IsAOAGFeatureEnabled_IsCloudInstance_UseEDDSResource()
		{
			// arrange
			_toggle.Configuration.IsCloudInstance.Returns(true);
			_toggle.Configuration.UseEDDSResource.Returns(true);

			// act
			bool result = _toggle.IsAOAGFeatureEnabled();

			// assert 
			Assert.IsTrue(result);
		}

		[Test]
		public void IsAOAGFeatureEnabled_NotCloudInstance_UseEDDSResource()
		{
			// arrange
			_toggle.Configuration.IsCloudInstance.Returns(false);
			_toggle.Configuration.UseEDDSResource.Returns(true);

			// act
			bool result = _toggle.IsAOAGFeatureEnabled();

			// assert 
			Assert.IsFalse(result);
		}

		[Test]
		public void IsAOAGFeatureEnabled_NotCloudInstance_NotUseEDDSResource()
		{
			// arrange
			_toggle.Configuration.IsCloudInstance.Returns(false);
			_toggle.Configuration.UseEDDSResource.Returns(false);

			// act
			bool result = _toggle.IsAOAGFeatureEnabled();

			// assert 
			Assert.IsTrue(result);
		}
	}
}