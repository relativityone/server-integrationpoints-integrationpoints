using System;
using System.Reflection;
using kCura.Apps.Common.Utils.Serializers;
using kCura.Relativity.DataReaderClient;
using NSubstitute;
using NUnit.Framework;
using Relativity.Core;
using Relativity.Core.Service;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Unit
{
	[TestFixture]
	public class ImportSettingsTests
	{
		private IAuditSpoofTokenGenerator _generator;
		private BaseServiceContext _context;

		[SetUp]
		public void Setup()
		{
			_generator = NSubstitute.Substitute.For<IAuditSpoofTokenGenerator>();
			_context = NSubstitute.Substitute.For<BaseServiceContext>();
		}

		[Test]
		public void ImportSettings_SerializeDesirialize()
		{
			//ARRANGE
			kCura.Apps.Common.Utils.Serializers.JSONSerializer serializer = new JSONSerializer();
			ImportSettings settings = new ImportSettings();
			settings.ImportOverwriteMode = ImportOverwriteModeEnum.AppendOverlay;


			//ACT
			string serializedString = serializer.Serialize(settings);
			serializer.Deserialize<ImportSettings>(serializedString);


			//ASSERT
			Assert.IsFalse(serializedString.Contains("\"AuditLevel\""));
			Assert.IsFalse(serializedString.Contains("\"NativeFileCopyMode\""));
			Assert.IsFalse(serializedString.Contains("\"OverwriteMode\""));
			Assert.IsFalse(serializedString.Contains("\"OverlayBehavior\""));
			Assert.AreEqual(OverwriteModeEnum.AppendOverlay, (OverwriteModeEnum)GetPropertyValue(settings, "OverwriteMode"));
		}

		[Test]
		public void ImportSettings_OnBehalfOfUserToken()
		{
			ImportSettings setting = new ImportSettings(_generator, _context);
			setting.OnBehalfOfUserId = 777;

			string token = setting.OnBehalfOfUserToken;

			_generator.Received(1).Create(_context, setting.OnBehalfOfUserId);
		}

		[Test]
		public void ImportSettings_OnBehalfOfUserToken_NoUserIdSet()
		{
			ImportSettings setting = new ImportSettings(_generator, _context);

			string token = setting.OnBehalfOfUserToken;

			Assert.IsEmpty(token);
			_generator.DidNotReceive().Create(Arg.Any<BaseServiceContext>(), Arg.Any<int>());
		}

		[TestCase(null, Result = ImportOverlayBehaviorEnum.UseRelativityDefaults)]
		[TestCase("", Result = ImportOverlayBehaviorEnum.UseRelativityDefaults)]
		[TestCase("Use Field Settings", Result = ImportOverlayBehaviorEnum.UseRelativityDefaults)]
		[TestCase("Merge Values", Result = ImportOverlayBehaviorEnum.MergeAll)]
		[TestCase("Replace Values", Result = ImportOverlayBehaviorEnum.ReplaceAll)]
		[TestCase("exception please", ExpectedException = typeof(Exception))]
		public ImportOverlayBehaviorEnum ImportSettings_ImportOverlayBehavior(string input)
		{
			ImportSettings setting = new ImportSettings(_generator, _context);
			setting.FieldOverlayBehavior = input;
			return setting.ImportOverlayBehavior;
		}

		private object GetPropertyValue(object o, string propertyName)
		{
			BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			PropertyInfo property = o.GetType().GetProperty(propertyName, bindFlags);
			return property.GetValue(o);
		}
	}
}
