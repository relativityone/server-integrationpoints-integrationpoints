using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using System.Security.Claims;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
	internal class UserHelper
	{
		private RelativityUser _testUser;

		private const int _ADMIN_USER_ID = 9;
		
		private readonly TestContext _testContext;

		public UserHelper(TestContext testContext)
		{
			_testContext = testContext;
		}

		public RelativityUser GetOrCreateTestUser(string timeStamp)
		{
			if (_testUser != null)
			{
				return _testUser;
			}

			_testUser = SharedVariables.UiSkipUserCreation
				? new RelativityUser(SharedVariables.RelativityUserName, SharedVariables.RelativityPassword)
				: CreateNewTestUser(timeStamp);

			return _testUser;
		}

		public void DeleteUserIfWasCreated()
		{
			if (_testUser != null && _testUser.CreatedInTest)
			{
				User.DeleteUser(_testUser.ArtifactId);
			}
		}

		private RelativityUser CreateNewTestUser(string timeStamp)
		{
			ClaimsPrincipal.ClaimsPrincipalSelector += () =>
			{
				var factory = new ClaimsPrincipalFactory();
				return factory.CreateClaimsPrincipal2(_ADMIN_USER_ID, _testContext.Helper);
			};

			UserModel userModel = User.CreateUser("RIP", $"Test_User_{timeStamp}", $"RIP_Test_User_{timeStamp}@relativity.com");
			return new RelativityUser(userModel.ArtifactID, userModel.EmailAddress, userModel.Password);
		}
	}
}
