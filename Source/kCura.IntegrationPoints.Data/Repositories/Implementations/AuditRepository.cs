using System;
using System.Collections.Generic;
using System.Xml.Linq;
using kCura.IntegrationPoints.Data.Models;
using Relativity.API;
using Relativity.API.Foundation;
using Relativity.API.Foundation.Repositories;
using Relativity.Data.MassImport;
using Relativity.MassImport;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
	public class AuditRepository : IAuditRepository
	{
//		private const string _SYSTEM_IDENTIFIER = "System";
//
//		private readonly global::Relativity.API.Foundation.Repositories.IAuditRepository _foundationAuditRepository;
//		private readonly ISystemArtifactCacheRepository _systemArtifactCacheRepository;
//
//		internal AuditRepository(global::Relativity.API.Foundation.Repositories.IAuditRepository foundationAuditRepository)
//		{
//			_foundationAuditRepository = foundationAuditRepository;
//			_systemArtifactCacheRepository = systemArtifactCacheRepository;
//		}

		public bool AuditExport(int appID, bool isFatalError, ExportStatistics exportStats)
		{
			// TODO
			throw new NotImplementedException();
		}
	}
}
