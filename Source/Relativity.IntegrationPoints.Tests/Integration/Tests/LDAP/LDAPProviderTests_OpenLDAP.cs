using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FluentAssertions;
using kCura.IntegrationPoints.LDAPProvider;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Tests.LDAP.TestData;
using Relativity.IntegrationPoints.Tests.Integration.Utils;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.LDAP
{
	[IdentifiedTestFixture("7D567FE8-720D-43B7-81A2-8200C6F10FAB")]
	[TestExecutionCategory.CI, TestLevel.L2]
	public class LDAPProviderTests_OpenLDAP : TestsBase
	{
		public readonly AdministrativeTestData AdministrativeTestData;
		public readonly ProductDevelopmentTestData ProductDevelopmentTestData;
		public readonly HumanResourcesTestData HumanResourcesTestData;

		public LDAPProviderTests_OpenLDAP()
		{
			AdministrativeTestData = new AdministrativeTestData();
			ProductDevelopmentTestData = new ProductDevelopmentTestData();
			HumanResourcesTestData = new HumanResourcesTestData();
		}

		[IdentifiedTest("1F1BA8DD-73A2-45CC-8659-8E1EE90D5CD0")]
		public void OpenLDAP_GetBatchableIds_ShouldReturnAllIds_WhenSimpleOU()
		{
			// Arrange
			IDataSourceProvider sut = PrepareSut();

			DataSourceProviderConfiguration sourceProviderConfiguration = PrepareOpenLDAPConfiguration("ou=Human Resources", AuthenticationTypesEnum.FastBind);

			// Act
			IDataReader reader = sut.GetBatchableIds(HumanResourcesTestData.IdentifierFieldEntry, sourceProviderConfiguration);

			IList<IDictionary<string, object>> result = ReaderUtil.Read(reader);

			// Assert
			object IdSelector(IDictionary<string, object> row) => row[HumanResourcesTestData.UniqueId];

			//We need to sanitize in case of child ou which are bypassed anyway in SyncManager
			result.Select(IdSelector).Where(x => x != null)
				.ShouldBeEquivalentTo(HumanResourcesTestData.EntryIds);
		}

		[IdentifiedTest("9CE157C0-C387-4C60-91E0-84C5BCAB9D25")]
		public void OpenLDAP_GetBatchableIds_ShouldReturnAllIds_WhenNestedImportIsRequested()
		{
			// Arrange
			IDataSourceProvider sut = PrepareSut();

			DataSourceProviderConfiguration sourceProviderConfiguration = PrepareOpenLDAPConfiguration("ou=Administrative", AuthenticationTypesEnum.FastBind, importNested: true);

			// Act
			IDataReader reader = sut.GetBatchableIds(AdministrativeTestData.IdentifierFieldEntry, sourceProviderConfiguration);

			IList<IDictionary<string, object>> result = ReaderUtil.Read(reader);

			// Assert
			object IdSelector(IDictionary<string, object> row) => row[AdministrativeTestData.UniqueId];

			//We need to sanitize in case of child ou which are bypassed anyway in SyncManager
			result.Select(IdSelector).Where(x => x != null)
				.ShouldBeEquivalentTo(
					AdministrativeTestData.EntryIds.Concat(ProductDevelopmentTestData.EntryIds));
		}

		[IdentifiedTest("DDF99717-FADE-441A-9CB6-F9295181DD7A")]
		public void OpenLDAP_GetBatchableIds_ShouldReturnAllIds_WhenChildOU()
		{
			// Arrange
			IDataSourceProvider sut = PrepareSut();

			DataSourceProviderConfiguration sourceProviderConfiguration = PrepareOpenLDAPConfiguration("ou=Product Development,ou=Administrative", AuthenticationTypesEnum.FastBind);

			// Act
			IDataReader reader = sut.GetBatchableIds(ProductDevelopmentTestData.IdentifierFieldEntry, sourceProviderConfiguration);

			IList<IDictionary<string, object>> result = ReaderUtil.Read(reader);

			// Assert
			object IdSelector(IDictionary<string, object> row) => row[ProductDevelopmentTestData.UniqueId];

			//We need to sanitize in case of child ou which are bypassed anyway in SyncManager
			result.Select(IdSelector).Where(x => x != null)
				.ShouldBeEquivalentTo(ProductDevelopmentTestData.EntryIds);
		}

		[IdentifiedTest("02195D3F-5E29-46B3-BEC4-92C7228605CC")]
		public void OpenLDAP_GetFields_ShouldReturnAllProperties()
		{
			// Arrange
			IDataSourceProvider sut = PrepareSut();

			DataSourceProviderConfiguration sourceProviderConfiguration = PrepareOpenLDAPConfiguration("ou=Human Resources", AuthenticationTypesEnum.FastBind);

			// Act
			IEnumerable<FieldEntry> fields = sut.GetFields(sourceProviderConfiguration);

			// Assert
			AssertProperties(HumanResourcesTestData, fields);
		}

		[IdentifiedTest("D7818545-AB77-487D-A032-CCEC57AEFF2A")]
		public void OpenLDAP_GetData_ShouldReturnData_WhenSimpleOU()
		{
			// Arrange
			IDataSourceProvider sut = PrepareSut();

			DataSourceProviderConfiguration sourceProviderConfiguration = PrepareOpenLDAPConfiguration("ou=Human Resources", AuthenticationTypesEnum.FastBind);

			IEnumerable<FieldEntry> fieldEntries = HumanResourcesTestData.GetFieldEntries();

			IEnumerable<string> entryIds = HumanResourcesTestData.EntryIds;

			// Act
			IDataReader reader = sut.GetData(fieldEntries, entryIds, sourceProviderConfiguration);

			IList<IDictionary<string, object>> result = ReaderUtil.Read(reader);
			
			// Assert
			AssertData(HumanResourcesTestData.Data, result);
		}

		[IdentifiedTest("5078925F-29D3-4542-B3B4-52B770A36CB4")]
		public void OpenLDAP_GetData_ShouldReturnData_WhenNestedImportIsRequested()
		{
			// Arrange
			IDataSourceProvider sut = PrepareSut();

			DataSourceProviderConfiguration sourceProviderConfiguration = PrepareOpenLDAPConfiguration("ou=Administrative", AuthenticationTypesEnum.FastBind, importNested: true);

			IEnumerable<FieldEntry> fieldEntries = AdministrativeTestData.GetFieldEntries();

			IEnumerable<string> entryIds = AdministrativeTestData.EntryIds.Concat(ProductDevelopmentTestData.EntryIds);

			// Act
			IDataReader reader = sut.GetData(fieldEntries, entryIds, sourceProviderConfiguration);

			IList<IDictionary<string, object>> result = ReaderUtil.Read(reader);

			// Assert
			AssertData(AdministrativeTestData.Data.Concat(ProductDevelopmentTestData.Data), result);
		}

		[IdentifiedTest("61C6ED38-90B4-45CA-A647-BA8286A1EDA4")]
		public void OpenLDAP_GetData_ShouldReturnData_WhenChildOU()
		{
			// Arrange
			IDataSourceProvider sut = PrepareSut();

			DataSourceProviderConfiguration sourceProviderConfiguration = PrepareOpenLDAPConfiguration("ou=Product Development,ou=Administrative", AuthenticationTypesEnum.FastBind);

			IEnumerable<FieldEntry> fieldEntries = ProductDevelopmentTestData.GetFieldEntries();

			IEnumerable<string> entryIds = ProductDevelopmentTestData.EntryIds;

			// Act
			IDataReader reader = sut.GetData(fieldEntries, entryIds, sourceProviderConfiguration);

			IList<IDictionary<string, object>> result = ReaderUtil.Read(reader);

			// Assert
			AssertData(ProductDevelopmentTestData.Data, result);
		}

		[IdentifiedTest("F30A6BDD-CCFD-4CBC-90C6-F526E7584A48")]
		public void OpenLDAP_GetData_ShouldReturnDateTimeAsString()
		{
			// Arrange
			const string dateTimeProp = "createTimestamp";

			IDataSourceProvider sut = PrepareSut();

			DataSourceProviderConfiguration sourceProviderConfiguration = PrepareOpenLDAPConfiguration("ou=Human Resources", AuthenticationTypesEnum.FastBind);

			IEnumerable<FieldEntry> fieldEntries = new[]
			{
				HumanResourcesTestData.IdentifierFieldEntry,
				GetFieldEntry(dateTimeProp)
			};

			IEnumerable<string> entryIds = HumanResourcesTestData.EntryIds;

			// Act
			IDataReader reader = sut.GetData(fieldEntries, entryIds, sourceProviderConfiguration);

			IList<IDictionary<string, object>> result = ReaderUtil.Read(reader);

			// Assert
			IList<object> dateTimeResults = result.Select(x => x[dateTimeProp]).ToList();
			
			dateTimeResults.Should().AllBeOfType<string>();
		}

		[IdentifiedTest("595F160F-55BF-4712-8280-D5DD669FB9F5")]
		public void OpenLDAP_GetData_ShouldReturnMultiValueAsStringWithDelimiter()
		{
			// Arrange
			const char delimiter = ';';
			const string multiValueProp = "objectClass";

			IDataSourceProvider sut = PrepareSut();

			DataSourceProviderConfiguration sourceProviderConfiguration = PrepareOpenLDAPConfiguration(
				"ou=Human Resources", AuthenticationTypesEnum.FastBind, multiValueDelimiter: delimiter);


			IEnumerable<FieldEntry> fieldEntries = new[]
			{
				HumanResourcesTestData.IdentifierFieldEntry,
				GetFieldEntry(multiValueProp)
			};

			IEnumerable<string> entryIds = HumanResourcesTestData.EntryIds;

			// Act
			IDataReader reader = sut.GetData(fieldEntries, entryIds, sourceProviderConfiguration);

			IList<IDictionary<string, object>> result = ReaderUtil.Read(reader);

			// Assert
			result.Select(x => x[multiValueProp]).Should()
				.OnlyContain(x => x.ToString().Split(delimiter).Length > 0);
		}

		[IdentifiedTest("F0FCEC3A-69C8-41C1-9789-7B16BD1A1FAC")]
		public void OpenLDAP_GetData_ShouldReturnByteArrayAsPreFormattedString()
		{
			// Arrange
			const string password = "Password1";
			const string passwordField = "userpassword";

			byte[] passwordBytes = Encoding.ASCII.GetBytes(password);

			IDataSourceProvider sut = PrepareSut();

			DataSourceProviderConfiguration sourceProviderConfiguration = PrepareOpenLDAPConfiguration(
				"ou=Human Resources", AuthenticationTypesEnum.FastBind);
			
			IEnumerable<FieldEntry> fieldEntries = new[]
			{
				HumanResourcesTestData.IdentifierFieldEntry,
				GetFieldEntry(passwordField)
			};

			IEnumerable<string> entryIds = HumanResourcesTestData.EntryIds;

			// Act
			IDataReader reader = sut.GetData(fieldEntries, entryIds, sourceProviderConfiguration);

			IList<IDictionary<string, object>> result = ReaderUtil.Read(reader);

			// Assert
			result.Select(x => x[passwordField]).Should()
				.OnlyContain(x => HexToByteArray(x.ToString()).SequenceEqual(passwordBytes));
		}

		private IDataSourceProvider PrepareSut()
		{
			return new LDAPProvider(
				Container.Resolve<ILDAPSettingsReader>(),
				Container.Resolve<ILDAPServiceFactory>(),
				Helper,
				Serializer);
		}

		private DataSourceProviderConfiguration PrepareOpenLDAPConfiguration(string ou, AuthenticationTypesEnum authType, bool importNested = false, char? multiValueDelimiter = '|')
		{
			LDAPSettings settings = new LDAPSettings
			{
				ConnectionPath = $"rip-openldap-cvnx78s.eastus.azurecontainer.io/{ou},dc=rip-openldap-cvnx78s,dc=eastus,dc=azurecontainer,dc=io",
				ConnectionAuthenticationType = authType,
				ImportNested = importNested,
				MultiValueDelimiter = multiValueDelimiter
			};

			LDAPSecuredConfiguration securedConfiguration = new LDAPSecuredConfiguration
			{
				UserName = "cn=admin,dc=rip-openldap-cvnx78s,dc=eastus,dc=azurecontainer,dc=io",
				Password = "Test1234!"
			};

			return new DataSourceProviderConfiguration(
				Serializer.Serialize(settings),
				Serializer.Serialize(securedConfiguration));
		}

		private FieldEntry GetFieldEntry(string propertyName) => 
			new FieldEntry
			{
				DisplayName = propertyName,
				FieldIdentifier = propertyName,
			};

		private static byte[] HexToByteArray(string hex)
		{
			int NumberChars = hex.Length;
			byte[] bytes = new byte[NumberChars / 2];
			for (int i = 0; i < NumberChars; i += 2)
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			return bytes;
		}

		private void AssertProperties(TestDataBase testData, IEnumerable<FieldEntry> actual)
		{
			string[] skippedProperties = new[]
			{
				"adspath",
				"userpassword"
			};

			actual
				.Where(x => !skippedProperties.Contains(x.FieldIdentifier))
				.ShouldAllBeEquivalentTo(testData.GetFieldEntries(),
					options => options
						.Including(o => o.DisplayName)
						.Including(o => o.FieldIdentifier));
		}

		private void AssertData(IEnumerable<IDictionary<string, object>> expected, IEnumerable<IDictionary<string, object>> actual)
		{
			actual.ShouldAllBeEquivalentTo(expected);
		}
	}
}
