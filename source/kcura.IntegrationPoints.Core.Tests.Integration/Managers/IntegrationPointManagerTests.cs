using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Managers
{
	[TestFixture]
	[Ignore("Tests don't work and need fix")]
	public class IntegrationPointManagerTests : RelativityProviderTemplate
	{
		private IDBContext _dbContext;
		private int _groupId;
		private UserModel _userModel;

		public IntegrationPointManagerTests() : base("IntegrationPointManagerSource", "IntegrationPointManagerTarget")
		{

		}

		public override void SuiteSetup()
		{
			InstanceSetting.UpsertAndReturnOldValueIfExists("Relativity.Authentication", "AdminsCanSetPasswords", "True");
			_dbContext = Helper.GetDBContext(-1);
			CreateGroupAndUser();
		}

		[Test]
		public void IntegrationCreateAndSavePermissions()
		{
			//Arrange
			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "IntegrationPoint" + DateTime.Now,
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				Map = CreateDefaultFieldMap()
			};

			//Change user context
			SharedVariables.RelativityUserName = _userModel.EmailAddress;
			SharedVariables.RelativityPassword = _userModel.Password;

			//Permission dictionary
			Dictionary<string, bool> permissions = new Dictionary<string, bool>();
			//permissions.Add("");

			//Act



			//Assert


		}

		private string GetErrorMessage (string expectedErrorMessage)
		{
			string query = "SELECT FROM [EDDS].[eddsdbo].[Error] WHERE CaseArtifactID = @caseArtifact AND Message = '@message'";

			SqlParameter[] parameters = new SqlParameter[]
			{
				new SqlParameter("caseArtifact", SqlDbType.NVarChar) { Value = SourceWorkspaceArtifactId },
				new SqlParameter("message", SqlDbType.NVarChar) { Value = expectedErrorMessage },
			};

			try
			{
				string fullMessage = _dbContext.ExecuteSqlStatementAsScalar<string>(query, parameters);
				return fullMessage;
			}
			catch (Exception ex)
			{
				throw new Exception($"An error occurred while querying for Relativity Error Message: { expectedErrorMessage } for Workspace: { SourceWorkspaceArtifactId }. Exception: { ex.Message }.");
			}
		}

		private void CreateGroupAndUser()
		{
			_groupId = Group.CreateGroup("Permission Test Group");
			_userModel = User.CreateUser("New", "Test", "permissionsTest@kcura.com", new[] { _groupId });
		}
	}
}
