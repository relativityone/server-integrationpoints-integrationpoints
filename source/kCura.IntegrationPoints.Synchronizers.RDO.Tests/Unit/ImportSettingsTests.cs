using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI.Data;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity;

namespace kCura.IntegrationPoints.Synchronizers.RDO.Tests.Unit
{
	[TestFixture]
	public class ImportSettingsTests
	{
		[Test]
		public void ImportSettings_SerializeDesirialize()
		{
			//ARRANGE
			kCura.Apps.Common.Utils.Serializers.JSONSerializer serializer = new JSONSerializer();
			ImportSettings settings = new ImportSettings();
			settings.ImportOverwriteMode = ImportOverwriteModeEnum.AppendOverlay;


			//ACT
			string serializedString = serializer.Serialize(settings);
			ImportSettings newSettings = serializer.Deserialize<ImportSettings>(serializedString);


			//ASSERT
			Assert.IsFalse(serializedString.Contains("\"AuditLevel\""));
			Assert.IsFalse(serializedString.Contains("\"NativeFileCopyMode\""));
			Assert.IsFalse(serializedString.Contains("\"OverwriteMode\""));
			Assert.IsFalse(serializedString.Contains("\"OverlayBehavior\""));
			Assert.AreEqual(OverwriteModeEnum.AppendOverlay, (OverwriteModeEnum)GetPropertyValue(settings, "OverwriteMode"));
		}

		private object GetPropertyValue(object o, string propertyName)
		{
			BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			PropertyInfo property = o.GetType().GetProperty(propertyName, bindFlags);
			return property.GetValue(o);
		}
	}
}
