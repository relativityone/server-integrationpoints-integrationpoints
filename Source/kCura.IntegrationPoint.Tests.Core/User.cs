using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Interfaces.UserInfo;
using Relativity.Services.Interfaces.UserInfo.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class User
    {
        private const int _SYSTEM_ADMINISTRATOR_GROUP_ID = 20;
        private const int _USER_TYPE_INTERNAL_ID = 663;
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
                var userRequest = new UserRequest()
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
                            ArtifactID = GetClientArtifactId()
                        }
                    },
                    Type = new ObjectIdentifier()
                    {
                        ArtifactID = _USER_TYPE_INTERNAL_ID
                    },
                    DocumentViewerProperties = new DocumentViewerProperties(),
                };

                UserResponse userResponse = userManager.CreateAsync(userRequest).GetAwaiter().GetResult();

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

                string password = Helper.RelativityPassword;
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

        private static int GetClientArtifactId()
        {
            using (IObjectManager objectManager = Helper.CreateProxy<IObjectManager>())
            {
                var clientRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef()
                    {
                        ArtifactTypeID = (int)ArtifactType.Client
                    }
                };

                QueryResultSlim result = objectManager.QuerySlimAsync(-1, clientRequest, 0, int.MaxValue).GetAwaiter().GetResult();

                if (result.ResultCount == 0)
                {
                    throw new NotFoundException("There are no clients available on this Relativity instance.");
                }

                return result.Objects.First().ArtifactID;
            }
        }
    }
}
