using System.Collections.Generic;
using System.Linq;

namespace Relativity.IntegrationPoints.Tests.Common.LDAP.TestData
{
    public class JumpCloudUsersTestData : TestDataBase
    {
        public JumpCloudUsersTestData() : base(nameof(JumpCloudUsersTestData), "uid")
        {
            AllProperties = new []
            {
                "cn",
                "gidnumber",
                "givenname",
                "homedirectory",
                "jcldapadmin",
                "loginshell",
                "mail",
                "objectclass",
                "sn",
                "uid",
                "uidnumber",
                "userpassword"
            };
        }

        public new IEnumerable<string> EntryIds => Data.Select(x => x["username"].ToString());

        public override string OU => "ou=Users";
    }
}
