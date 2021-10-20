using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.LDAPProvider;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Common;
using Relativity.IntegrationPoints.Tests.Common.LDAP.TestData;
using Relativity.IntegrationPoints.Tests.Integration.Utils;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.LDAP
{
	[TestExecutionCategory.CI, TestLevel.L2]
	public class LDAPProviderTests_JumpCloud : TestsBase
	{
		public readonly JumpCloudUsersTestData UsersTestData;

		private readonly string[] _SKIPPED_PROPERTIES = new[]
		{
			"adspath"
		};

		public LDAPProviderTests_JumpCloud()
		{
			UsersTestData = new JumpCloudUsersTestData();
		}

		[IdentifiedTestCase("CACF680A-1A88-43D0-8EB3-703D0A4E11E5", AuthenticationTypesEnum.FastBind, null)]
		[IdentifiedTestCase("497A6442-505A-4B0E-9185-CAA87C3E393D", AuthenticationTypesEnum.Encryption, 636)]
		public virtual void JumpCloud_GetFields_ShouldReturnAllProperties_BasedOnAuthenticationType(
			AuthenticationTypesEnum authType, int? port)
		{
			// Arrange
			IDataSourceProvider sut = PrepareSut();

			DataSourceProviderConfiguration sourceProviderConfiguration = PrepareJumpCloudLDAPConfiguration(authType, port);

			// Act
			IEnumerable<FieldEntry> fields = sut.GetFields(sourceProviderConfiguration);

			// Assert
			fields
				.Where(x => !_SKIPPED_PROPERTIES.Contains(x.FieldIdentifier))
				.ShouldAllBeEquivalentTo(UsersTestData.GetFieldEntries(),
					options => options
						.Including(o => o.DisplayName)
						.Including(o => o.FieldIdentifier));
		}

		[IdentifiedTest("80594D76-2B49-4AB7-AA12-1586E6F15CA1")]
		public virtual void JumpCloud_GetBatchableIds_ShouldReturnAllIds()
		{
			// Arrange
			const string uid = "uid";

			IDataSourceProvider sut = PrepareSut();

			DataSourceProviderConfiguration sourceProviderConfiguration = PrepareJumpCloudLDAPConfiguration(AuthenticationTypesEnum.Encryption, 636);

			FieldEntry identifier = new FieldEntry
			{
				FieldIdentifier = uid
			};

			// Act
			IDataReader reader = sut.GetBatchableIds(identifier, sourceProviderConfiguration);

			IList<IDictionary<string, object>> result = ReaderUtil.Read(reader);

			// Assert
			object IdSelector(IDictionary<string, object> row) => row[uid];

			result.Select(IdSelector).ShouldBeEquivalentTo(UsersTestData.EntryIds);
		}

		private IDataSourceProvider PrepareSut()
		{
			return new LDAPProvider(
				Container.Resolve<ILDAPSettingsReader>(),
				Container.Resolve<ILDAPServiceFactory>(),
				Helper,
				Serializer);
		}

		private DataSourceProviderConfiguration PrepareJumpCloudLDAPConfiguration(AuthenticationTypesEnum authType, int? port = null)
		{
			LDAPSettings settings = new LDAPSettings
			{
				ConnectionPath = GlobalConst.LDAP._JUMP_CLOUD_CONNECTION_PATH(port),
				ConnectionAuthenticationType = authType,
			};

			LDAPSecuredConfiguration securedConfiguration = new LDAPSecuredConfiguration
			{
				UserName = GlobalConst.LDAP._JUMP_CLOUD_USER,
				Password = GlobalConst.LDAP._JUMP_CLOUD_PASSWORD
			};

			return new DataSourceProviderConfiguration(
				Serializer.Serialize(settings),
				Serializer.Serialize(securedConfiguration));
		}
	}
}
