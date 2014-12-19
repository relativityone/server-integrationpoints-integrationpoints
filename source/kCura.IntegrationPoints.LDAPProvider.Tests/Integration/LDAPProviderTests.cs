using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using Newtonsoft.Json;
using NUnit.Framework;

namespace kCura.IntegrationPoints.LDAPProvider.Tests.Integration
{
	[TestFixture]
	public class LDAPProviderTests
	{
		[Test]
		[Explicit]
		public void GetFields_Test_PASS()
		{
			//ARRANGE
			LDAPSettings settings = new LDAPSettings();
			settings.ConnectionPath = "testing.corp";
			settings.ConnectionAuthenticationType = AuthenticationTypesEnum.FastBind;
			settings.UserName = "testing\\administrator";
			settings.Password = "P@ssw0rd@1";
			string options = new JSONSerializer().Serialize(settings);
			IDataSourceProvider ldap = new LDAPProvider();

			//ACT
			IEnumerable<FieldEntry> fields = ldap.GetFields(options);

			//ASSERT
			foreach (FieldEntry field in fields)
			{
				Console.WriteLine(field.DisplayName);
			}
		}
	}
}
