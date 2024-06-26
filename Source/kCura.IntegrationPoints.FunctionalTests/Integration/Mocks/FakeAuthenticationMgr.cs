﻿using Relativity.API;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks
{
    class FakeAuthenticationMgr : IAuthenticationMgr
    {
        public FakeAuthenticationMgr(FakeUser user)
        {
            UserInfo = new FakeUserInfo(user.ArtifactId);
        }

        public string GetAuthenticationToken()
        {
            return "FD905984-10D5-4467-8203-19A554B6525A";
        }

        public IUserInfo UserInfo { get; }
    }
}
