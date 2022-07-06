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
	}
}
