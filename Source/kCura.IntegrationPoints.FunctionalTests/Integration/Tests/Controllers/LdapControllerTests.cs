using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Controllers
{
	public class LdapControllerTests : TestsBase
	{	
		[IdentifiedTest("D13E2898-CC2E-4CFC-93B3-25C4398B7F32")]
		public void CheckLdap_ShouldAuthenticateLDAPConnection()
		{
			// Arrange
			SynchronizerSettings settings = PrepareOpenLDAPSettings();

			LdapController sut = Container.Resolve<LdapController>();

			// Act
			IHttpActionResult response = sut.CheckLdap(settings);

			// Assert
			response.Should().BeOfType<StatusCodeResult>();

			var result = (StatusCodeResult) response;
			result.StatusCode.Should().Be(HttpStatusCode.NoContent);
		}
		
		private SynchronizerSettings PrepareOpenLDAPSettings()
		{
			LDAPSettings settings = new LDAPSettings
			{
				ConnectionPath = "rip-openldap-cvnx78s.eastus.azurecontainer.io/ou=Administrative,dc=rip-openldap-cvnx78s,dc=eastus,dc=azurecontainer,dc=io",
				ConnectionAuthenticationType = AuthenticationTypesEnum.FastBind
			};

			LDAPSecuredConfiguration securedConfiguration = new LDAPSecuredConfiguration
			{
				UserName = "cn=admin,dc=rip-openldap-cvnx78s,dc=eastus,dc=azurecontainer,dc=io",
				Password = "Test1234!"
			};

			return new SynchronizerSettings
			{
				Credentials = Serializer.Serialize(securedConfiguration),
				Settings = Serializer.Serialize(settings)
			};
		}
	}
}
