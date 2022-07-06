using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.UITests.Logging;
using Relativity;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Script;
using Relativity.Services.Interfaces.Scripts.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Serilog;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
	internal class RelativityFolderPathScriptHelper
	{
		private const string _SCRIPT_NAME = "Set Relativity Folder Path Field";
		private readonly TestContext _testContext;
		private static readonly ILogger Log = LoggerFactory.CreateLogger(typeof(RelativityFolderPathScriptHelper));

		public RelativityFolderPathScriptHelper(TestContext testContext)
		{
			_testContext = testContext;
		}

		//public void ExecuteRelativityFolderPathScript()
		//{
		//	try
		//	{
		//		int workspaceId = _testContext.GetWorkspaceId();

		//		using (IScriptManager scriptManager = _testContext.Helper.CreateProxy<IScriptManager>())
		//		using (IObjectManager objectManager = _testContext.Helper.CreateProxy<IObjectManager>())
		//		{
		//			QueryResult queryResult = objectManager.QueryAsync(workspaceId, new QueryRequest()
		//			{
		//				ObjectType = new ObjectTypeRef()
		//				{
		//					ArtifactTypeID = (int)ArtifactType.RelativityScript
		//				},
		//				Condition = $"'Name' == '{_SCRIPT_NAME}'"
		//			}, 0, 1).GetAwaiter().GetResult();

		//			if (!queryResult.Objects.Any())
		//			{
		//				string message = $"Cannot find script: '{_SCRIPT_NAME}'";
		//				Log.Error(message);
		//				throw new NotFoundException(message);
		//			}

		//			EnqueueRunJobResponse response = scriptManager.EnqueueRunJobAsync(workspaceId, queryResult.Objects.First().ArtifactID, new List<ScriptInput>()
		//			{
		//				new SingleScriptInput()
		//				{
		//					ID = "FolderPath",
		//					Value = "DocumentFolderPath"
		//				}
		//			}, 0).GetAwaiter().GetResult();


		//			TimeSpan waitTime = TimeSpan.FromSeconds(2);
		//			TimeSpan scriptExecutionTimeout = TimeSpan.FromMinutes(2);
		//			bool jobExecutionFinished = false;

		//			do
		//			{
		//				if (scriptExecutionTimeout.TotalSeconds == 0)
		//				{
		//					throw new TimeoutException($"Script '{_SCRIPT_NAME}' execution has timed out. Make sure 'Script Run Manager' agent is enabled.");
		//				}

		//				Task.Delay(waitTime).GetAwaiter().GetResult();
		//				scriptExecutionTimeout = scriptExecutionTimeout.Subtract(waitTime);

		//				RunJob runJob = scriptManager.ReadRunJobAsync(workspaceId, response.RunJobID).GetAwaiter().GetResult();

		//				if (runJob.Status == RunJobStatus.FailedToComplete || runJob.Status == RunJobStatus.CompletedWithErrors)
		//				{
		//					throw new Exception($"Script '{_SCRIPT_NAME}' execution error: {runJob.ErrorMessage}");
		//				}

		//				jobExecutionFinished = runJob.Status == RunJobStatus.Completed;

		//			} while (!jobExecutionFinished);

		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Log.Error(ex, "Script execution error.");
		//		throw;
		//	}
		//}
	}
}
