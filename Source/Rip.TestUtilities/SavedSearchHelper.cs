using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Services.Field;
using Relativity.Services.Search;

namespace Rip.TestUtilities
{
	public class SavedSearchHelper
	{
		private readonly int _workspaceID;
		private readonly IKeywordSearchManager _keywordSearchManager;

		public SavedSearchHelper(int workspaceID, IKeywordSearchManager keywordSearchManager)
		{
			_workspaceID = workspaceID;
			_keywordSearchManager = keywordSearchManager;
		}

		public void DeleteSavedSearch(int searchArtifactID)
		{
			_keywordSearchManager.DeleteSingleAsync(_workspaceID, searchArtifactID);
		}
	}
}
