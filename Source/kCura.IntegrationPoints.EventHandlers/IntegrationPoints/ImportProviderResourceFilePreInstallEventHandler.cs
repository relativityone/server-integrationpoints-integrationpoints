using System;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;

using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints
{
	[Description("This is an event handler that removes Integration Points ImportProvider and ImportProvider.Parser DLL files that were ILMerged into the main assembly.")]
	[Guid("8CAD683A-0AAC-4EB7-94C9-AB4D1450656D")]
	[RunTarget(EventHandler.Helper.RunTargets.Instance)] 
	[RunOnce(true)]
	public class ImportProviderResourceFilePreInstallEventHandler : kCura.EventHandler.PreInstallEventHandler
	{
		private const string _IMPORT_PROVIDER_RESOURCE_QUERY_PARAMETER = @"kCura.IntegrationPoints.ImportProvider.%";

		private IAPILog _logger;

		internal IAPILog Logger
		{
			get
			{
				if (_logger == null)
				{
					_logger = Helper.GetLoggerFactory().GetLogger().ForContext<ImportProviderResourceFilePreInstallEventHandler>();
				}

				return _logger;
			}
		}

		/// <summary>
		/// Remove ImportProvider.dll and ImportProvider.Parser.dll Resource files.
		/// These were installed in previous versions of IntegrationPoints, but are no longer necessary because these namespaces have been ILMerged into the main assembly.
		/// This event handler is necessary to avoid an ApplicationInstallationException
		/// </summary>
		/// <returns></returns>
		public override Response Execute()
		{
			IDBContext eddsDbContext = Helper.GetDBContext(-1);
			try
			{
				eddsDbContext.BeginTransaction();
				
				//Query parameter setup
				List<SqlParameter> sqlParams = new List<SqlParameter>
						{
							new SqlParameter("@ResourceQueryParam", SqlDbType.Text) { Value = _IMPORT_PROVIDER_RESOURCE_QUERY_PARAMETER },
						};

				//Delete from ResourceFileData table (FK constraint on ResourceFile table)
				const string sqlResourceFileData =	@"DELETE FROM [EDDS].[eddsdbo].[ResourceFileData]
													WHERE [ArtifactId] in
													(select [ArtifactID] from [EDDS].[eddsdbo].[ResourceFile] where [Name] like @ResourceQueryParam)";
				eddsDbContext.ExecuteNonQuerySQLStatement(sqlResourceFileData, sqlParams);

				//Delete from ResourceFile table
				const string sqlResourceFile =	@"DELETE FROM [EDDS].[eddsdbo].[ResourceFile]
												WHERE [Name] like @ResourceQueryParam";
				eddsDbContext.ExecuteNonQuerySQLStatement(sqlResourceFile, sqlParams);

				eddsDbContext.CommitTransaction();

				return new Response
				{
					Message = "Resource Files Removed Successfully",
					Success = true
				};
			}
			catch (System.Exception ex)
			{
				eddsDbContext.RollbackTransaction();
				LogRemoveImportProviderResourceError(ex);

				return new Response
				{
					Exception = ex,
					Message = ex.Message,
					Success = false
				};
			}
		}

		#region Logging

		private void LogRemoveImportProviderResourceError(Exception exception)
		{
			Logger.LogError(exception, "Failed to remove ImportProvider resource DLL's");
		}

		#endregion
	}
}