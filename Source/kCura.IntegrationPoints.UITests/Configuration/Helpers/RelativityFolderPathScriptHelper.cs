using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.UITests.Logging;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
	internal class RelativityFolderPathScriptHelper
	{
		private readonly TestContext _testContext;

		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(RelativityFolderPathScriptHelper));

		public RelativityFolderPathScriptHelper(TestContext testContext)
		{
			_testContext = testContext;
		}

		public bool ExecuteRelativityFolderPathScript()
		{
			using (var client = _testContext.Helper.CreateAdminProxy<IRSAPIClient>())
			{
				client.APIOptions.WorkspaceID = _testContext.GetWorkspaceId();

				RelativityScript relativityScript = FindRelativityFolderPathScript(client);
				Assert.That(relativityScript, Is.Not.Null, "Cannot find Relativity Script to set folder paths");

				var inputParameter = new RelativityScriptInput("FolderPath", "DocumentFolderPath");

				try
				{
					RelativityScriptResult scriptResult = client.Repositories.RelativityScript.ExecuteRelativityScript(relativityScript, new List<RelativityScriptInput>() { inputParameter });

					if (!scriptResult.Success)
					{
						Log.Error(@"Execution of Relativity Script failed: {0}", scriptResult.Message);
						return false;
					}
				}
				catch (Exception ex)
				{
					Log.Error(ex, @"An error occurred during executing Relativity Script: {0}", ex.Message);
					return false;
				}
			}
			return true;
		}

		private RelativityScript FindRelativityFolderPathScript(IRSAPIClient proxy)
		{
			var nameCondition = new TextCondition(RelativityScriptFieldNames.Name, TextConditionEnum.EqualTo,
				"Set Relativity Folder Path Field");
			var relScriptQuery = new Query<RelativityScript>()
			{
				Condition = nameCondition,
				Fields = FieldValue.NoFields
			};

			try
			{
				QueryResultSet<RelativityScript> relScriptQueryResults = proxy.Repositories.RelativityScript.Query(relScriptQuery);

				if (!relScriptQueryResults.Success)
				{
					Log.Error(@"An error occurred finding the script: {0}", relScriptQueryResults.Message);
				}
				return relScriptQueryResults.Results[0].Artifact;
			}
			catch (Exception ex)
			{
				Log.Error("An error occurred querying for Relativity Scripts: {0}", ex.Message);
			}
			return null;
		}
	}
}
