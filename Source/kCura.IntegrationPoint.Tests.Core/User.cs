using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;
using Artifact = kCura.Relativity.Client.DTOs.Artifact;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class User
	{
		private const int _SYSTEM_ADMINISTRATOR_GROUP_ID = 20;
		private static ITestHelper Helper => new TestHelper();

		public static UserModel CreateUser(string firstName, string lastName, string emailAddress, IList<int> groupIds = null)
		{
			IEnumerable<Relativity.Client.DTOs.Group> groups = GetGroupsForUser(groupIds);

			var userToCreate = new Relativity.Client.DTOs.User
			{
				ArtifactTypeID = 2,
				ArtifactTypeName = "User",
				ParentArtifact = new Artifact(20),
				Groups = new FieldValueList<Relativity.Client.DTOs.Group>(groups),
				FirstName = firstName,
				LastName = lastName,
				EmailAddress = emailAddress,
				Type = new Relativity.Client.DTOs.Choice(663)
				{
					ArtifactTypeID = 7,
					ArtifactTypeName = "Choice"
				},
				ItemListPageLength = 25,
				Client = new Client(1006066)
				{
					ArtifactTypeID = 5,
					ArtifactTypeName = "Client"
				},
				AuthenticationData = string.Empty,
				DefaultSelectedFileType = new Relativity.Client.DTOs.Choice(1014420)
				{
					ArtifactTypeID = 7,
					ArtifactTypeName = "Choice"
				},
				BetaUser = false,
				ChangeSettings = true,
				TrustedIPs = string.Empty,
				RelativityAccess = true,
				AdvancedSearchPublicByDefault = false,
				NativeViewerCacheAhead = true,
				ChangePassword = true,
				MaximumPasswordAge = 0,
				ChangePasswordNextLogin = false,
				SendPasswordTo = new Relativity.Client.DTOs.Choice(1015049)
				{
					ArtifactTypeID = 7,
					ArtifactTypeName = "Choice"
				},
				PasswordAction = new Relativity.Client.DTOs.Choice(1015048)
				{
					ArtifactTypeID = 7,
					ArtifactTypeName = "Choice"
				},
				Password = "Test1234!",
				DocumentSkip = new Relativity.Client.DTOs.Choice(1015042)
				{
					ArtifactTypeID = 7,
					ArtifactTypeName = "Choice"
				},
				DataFocus = 1,
				KeyboardShortcuts = true,
				EnforceViewerCompatibility = true,
				SkipDefaultPreference = new Relativity.Client.DTOs.Choice(1015044)
				{
					ArtifactTypeID = 7,
					ArtifactTypeName = "Choice"
				}
			};

			int createdUserArtifactId;
			using (IRSAPIClient rsapiClient = Rsapi.CreateRsapiClient())
			{
				WriteResultSet<Relativity.Client.DTOs.User> result = rsapiClient.Repositories.User.Create(userToCreate);
				createdUserArtifactId = result.Results.Single().Artifact.ArtifactID;
			}

			CreateLoginProfile(createdUserArtifactId, userToCreate.EmailAddress);

			return new UserModel(createdUserArtifactId, userToCreate.EmailAddress, userToCreate.Password);
		}

		public static void DeleteUser(int userArtifactId)
		{
			if (userArtifactId == 0)
			{
				return;
			}

			using (IRSAPIClient rsapiClient = Rsapi.CreateRsapiClient())
			{
				rsapiClient.Repositories.User.Delete(userArtifactId);
			}
		}

		private static IEnumerable<Relativity.Client.DTOs.Group> GetGroupsForUser(IList<int> groupIds)
		{
			groupIds = groupIds ?? new List<int> { _SYSTEM_ADMINISTRATOR_GROUP_ID };
			return groupIds.Select(groupId => new Relativity.Client.DTOs.Group(groupId));
		}

		private static void CreateLoginProfile(int userArtifactId, string userEmail)
		{
			using (var manager = Helper.CreateAdminProxy<ILoginProfileManager>())
			{
				manager.SaveLoginProfileAsync(new LoginProfile
				{
					Password = new PasswordMethod
					{
						Email = userEmail,
						InvalidLoginAttempts = 0,
						IsEnabled = true,
						MustResetPasswordOnNextLogin = false,
						PasswordExpirationInDays = 0,
						TwoFactorMode = TwoFactorMode.None,
						UserCanChangePassword = true
					},
					UserId = userArtifactId
				}).Wait();
			}
		}
	}
}