using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Interfaces.UserInfo;
using Relativity.Services.Interfaces.UserInfo.Models;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class User
	{
		private const int _SYSTEM_ADMINISTRATOR_GROUP_ID = 20;
		private static ITestHelper Helper => new TestHelper();

		public static UserModel CreateUser(string firstName, string lastName, string emailAddress, IList<int> groupIds = null)
		{
			if (groupIds == null || groupIds.Count == 0)
			{
				groupIds = new List<int>()
				{
					_SYSTEM_ADMINISTRATOR_GROUP_ID
				};
			}

			using (IUserInfoManager userManager = Helper.CreateProxy<IUserInfoManager>())
			using (ILoginProfileManager loginProfileManager = Helper.CreateProxy<ILoginProfileManager>())
			using (IGroupManager groupManager = Helper.CreateProxy<IGroupManager>())
			{
				const string password = "Test1234!";

				UserResponse userResponse = userManager.CreateAsync(new UserRequest()
				{
					FirstName = firstName,
					LastName = lastName,
					EmailAddress = emailAddress,
					ItemListPageLength = 200,
					AllowSettingsChange = true,
					RelativityAccess = true,
					Client = new Securable<ObjectIdentifier>()
					{
						Value = new ObjectIdentifier()
						{
							ArtifactID = 1006066
						}
					},
					Type = new ObjectIdentifier()
					{
						ArtifactID = 663
					},
					DocumentViewerProperties = new DocumentViewerProperties()
					{
					},
				}).GetAwaiter().GetResult();

				loginProfileManager.SaveLoginProfileAsync(new LoginProfile()
				{
					UserId = userResponse.ArtifactID,
					Password = new PasswordMethod()
					{
						Email = emailAddress,
						IsEnabled = true,
						UserCanChangePassword = true,
						MustResetPasswordOnNextLogin = false
					}
				}).GetAwaiter().GetResult();
				loginProfileManager.SetPasswordAsync(userResponse.ArtifactID, password).GetAwaiter().GetResult();

				foreach (int groupId in groupIds)
				{
					groupManager.AddMembersAsync(groupId, new ObjectIdentifier()
					{
						ArtifactID = userResponse.ArtifactID
					}).GetAwaiter().GetResult();
				}

				return new UserModel(userResponse.ArtifactID, userResponse.EmailAddress, password);
			}
		}
		
		public static void DeleteUser(int userArtifactId)
		{
			if (userArtifactId == 0)
			{
				return;
			}

			using (IUserInfoManager userManager = Helper.CreateProxy<IUserInfoManager>())
			{
				userManager.DeleteAsync(userArtifactId).GetAwaiter().GetResult();
			}
		}

	}
}
