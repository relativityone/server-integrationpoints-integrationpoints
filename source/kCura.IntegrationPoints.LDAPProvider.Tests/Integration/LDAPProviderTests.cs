using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
				Debug.WriteLine(field.DisplayName);
			}
		}

		[Test]
		[Explicit]
		public void GetBatchableIds_Test_PASS()
		{
			//ARRANGE
			LDAPSettings settings = new LDAPSettings();
			settings.ConnectionPath = "testing.corp/OU=Testing - Users,DC=testing,DC=corp";
			settings.ConnectionAuthenticationType = AuthenticationTypesEnum.FastBind;
			settings.UserName = "testing\\administrator";
			settings.Password = "P@ssw0rd@1";
			string options = new JSONSerializer().Serialize(settings);
			IDataSourceProvider ldap = new LDAPProvider();

			//ACT
			IDataReader sourceReader = ldap.GetBatchableIds(new FieldEntry() { DisplayName = "objectGUID", FieldIdentifier = "objectGUID" }, options);//bytearray-hex
			//IDataReader sourceReader = ldap.GetBatchableIds(new FieldEntry() { DisplayName = "whenCreated", FieldIdentifier = "whenCreated" }, options);//date
			//IDataReader sourceReader = ldap.GetBatchableIds(new FieldEntry() { DisplayName = "fromEntry", FieldIdentifier = "fromEntry" }, options);//bool
			//IDataReader sourceReader = ldap.GetBatchableIds(new FieldEntry() { DisplayName = "instanceType", FieldIdentifier = "instanceType" }, options);//int
			//IDataReader sourceReader = ldap.GetBatchableIds(new FieldEntry() { DisplayName = "uSNChanged", FieldIdentifier = "uSNChanged" }, options);//large
			//IDataReader sourceReader = ldap.GetBatchableIds(new FieldEntry() { DisplayName = "objectSid", FieldIdentifier = "objectSid" }, options);//SID
			//IDataReader sourceReader = ldap.GetBatchableIds(new FieldEntry() { DisplayName = "objectCategory", FieldIdentifier = "objectCategory" }, options);//Distinguished Name
			//IDataReader sourceReader = ldap.GetBatchableIds(new FieldEntry() { DisplayName = "objectClass", FieldIdentifier = "objectClass" }, options); //Object Identifier

			//ASSERT
			Object[] values = new Object[sourceReader.FieldCount];

			while (sourceReader.Read())
			{
				int fieldCount = sourceReader.GetValues(values);

				for (int i = 0; i < fieldCount; i++)
					Debug.WriteLine(values[i]);
			}

			// Always call Close when done reading.
			sourceReader.Close();
		}

		[Test]
		[Explicit]
		public void GetData_Test_PASS()
		{
			//ARRANGE
			LDAPSettings settings = new LDAPSettings();
			settings.ConnectionPath = "testing.corp/OU=Testing - Users,DC=testing,DC=corp";
			settings.ConnectionAuthenticationType = AuthenticationTypesEnum.FastBind;
			settings.UserName = "testing\\administrator";
			settings.Password = "P@ssw0rd@1";
			string options = new JSONSerializer().Serialize(settings);

			IEnumerable<Contracts.Models.FieldEntry> fields = new List<FieldEntry>()
			{
				new FieldEntry() {FieldIdentifier = "objectGUID",IsIdentifier = true},
				new FieldEntry() {FieldIdentifier = "givenName"},
				new FieldEntry() {FieldIdentifier = "sn"},
				new FieldEntry() {FieldIdentifier = "whenCreated"},
				new FieldEntry() {FieldIdentifier = "userPrincipalName"},
				new FieldEntry() {FieldIdentifier = "uSNCreated"},
			};
			IEnumerable<string> entryIds = new List<string>()
			{
				@"\19\2A\95\3F\87\9C\8B\4A\BF\A3\72\0F\D0\4C\A1\71"
				,@"\1A\28\AF\97\78\42\17\45\88\7B\D3\73\89\5F\EE\DD"
				,@"\21\91\5F\BF\39\B4\20\4F\9E\0F\35\EC\6C\48\A7\3B"
				,@"\20\BB\F4\54\30\8B\79\46\B9\74\C3\9A\D6\DD\58\86"
				,@"\44\61\DE\80\03\91\3D\41\94\89\D7\ED\E2\B7\BA\3A"
				,@"\22\88\76\CF\51\EF\35\43\88\87\81\94\44\51\AB\03"
				,@"\74\B1\DD\90\2C\E7\8B\4B\99\24\36\D4\FC\DA\DE\9F"
			};

			IDataSourceProvider ldap = new LDAPProvider();

			//ACT
			IDataReader sourceReader = ldap.GetData(fields, entryIds, options);


			//ASSERT
			Object[] values = new Object[sourceReader.FieldCount];
			DataTable dt = sourceReader.GetSchemaTable();
			int record = 0;
			while (sourceReader.Read())
			{
				record++;
				Debug.WriteLine(string.Format("record {0}:", record));
				int fieldCount = sourceReader.GetValues(values);

				for (int i = 0; i < fieldCount; i++)
					Debug.WriteLine(string.Format("{0}: {1}", dt.Columns[i], values[i]));
			}

			// Always call Close when done reading.
			sourceReader.Close();
		}
	}
}
