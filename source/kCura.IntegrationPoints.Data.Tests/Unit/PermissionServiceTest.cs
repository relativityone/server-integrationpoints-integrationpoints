using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Unit
{
	[TestFixture]
	public class PermissionServiceTest
	{
		[Test]
		public void userCanImport_userId_UserHasPermissionToImport()
		{
			//ARRANGE
			var context = NSubstitute.Substitute.For<IWorkspaceDBContext>();
			var permission = new PermissionService(context);
			var table = new DataTable("Users");
			table.Columns.Add("UserArtifactID", typeof(int));
			table.Rows.Add("123");

			//ACT
			context.ExecuteSqlStatementAsDataTable(Arg.Any<string>(),Arg.Any<List<SqlParameter>>()).Returns(table);
			//ASSERT 
			Assert.IsTrue(permission.userCanImport(123)); 
		}

		[Test]
		public void userCanImport_userId_UserDoesNotHavePermissionToImport()
		{
			//ARRANGE
			var context = NSubstitute.Substitute.For<IWorkspaceDBContext>();
			var permission = new PermissionService(context);
			var table = new DataTable("Users");
			table.Columns.Add("UserArtifactID", typeof(int));
		

			//ACT
			context.ExecuteSqlStatementAsDataTable(Arg.Any<string>(), Arg.Any<List<SqlParameter>>()).Returns(table);
			//ASSERT 
			Assert.IsFalse(permission.userCanImport(123));
		}
	}
}
