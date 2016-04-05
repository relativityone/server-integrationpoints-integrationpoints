﻿using kCura.IntegrationPoints.Core.Services.ServiceContext;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
	public class WorkspaceNameKeyword : IKeyword
	{
		private readonly ICaseServiceContext _context;
		private readonly Data.Queries.WorkspaceQuery _query;

		public string KeywordName { get { return "\\[WORKSPACE.NAME]"; } }

		public WorkspaceNameKeyword(ICaseServiceContext context, Data.Queries.WorkspaceQuery query)
		{
			_context = context;
			_query = query;
		}
		
		public string Convert()
		{
			return _query.GetWorkspaceName(_context.WorkspaceID);
		}
	}
}
