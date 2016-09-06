using System;
using kCura.EDDS.WebAPI.ExportManagerBase;
using kCura.WinEDDS.Service.Export;
using Relativity;
using Relativity.Core;
using Relativity.Core.Exception;
using Permission = Relativity.Core.Permission;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class CoreExportManager : IExportManager
	{
		private readonly BaseServiceContext _baseServiceContext;

		private readonly string[] _dynamicallyLoadedDlls;
		private readonly UserPermissionsMatrix _userPermissionsMatrix;

		public CoreExportManager(BaseServiceContext baseServiceContext)
		{
			_baseServiceContext = baseServiceContext;
			_userPermissionsMatrix = new UserPermissionsMatrix(baseServiceContext);
			_dynamicallyLoadedDlls = global::Relativity.Core.Api.Settings.RSAPI.Config.DynamicallyLoadedDllPaths;
		}

		public object[] RetrieveResultsBlock(int appID, Guid runId, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter,
			char nestedValueDelimiter, int[] textPrecedenceAvfIds)
		{
			_baseServiceContext.AppArtifactID = appID;
			var export = CreateExport(artifactTypeID, textPrecedenceAvfIds);
			var result = export.RetrieveResultsBlock(_baseServiceContext, runId, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter, nestedValueDelimiter);
			return result;
		}

		public object[] RetrieveResultsBlockForProduction(int appID, Guid runId, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested,
			char multiValueDelimiter, char nestedValueDelimiter, int[] textPrecedenceAvfIds, int productionId)
		{
			_baseServiceContext.AppArtifactID = appID;
			var export = CreateExport(artifactTypeID, textPrecedenceAvfIds);
			var result = export.RetrieveResultsBlockForProduction(_baseServiceContext, runId, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter,
				nestedValueDelimiter, productionId);
			return result;
		}

		public InitializationResults InitializeFolderExport(int appID, int viewArtifactID, int parentArtifactID, bool includeSubFolders, int[] avfIds, int startAtRecord,
			int artifactTypeID)
		{
			_baseServiceContext.AppArtifactID = appID;
			var export = CreateExport(artifactTypeID);
			CheckExportPermissions(export);
			var result = export.InitializeFolderExport(viewArtifactID, parentArtifactID, includeSubFolders, _dynamicallyLoadedDlls, avfIds, startAtRecord);

			return CreateInitializationResults(result);
		}

		public InitializationResults InitializeProductionExport(int appID, int productionArtifactID, int[] avfIds, int startAtRecord)
		{
			_baseServiceContext.AppArtifactID = appID;
			var export = CreateExport((int) ArtifactType.Document);
			CheckExportPermissions(export);
			var result = export.InitializeProductionExport(productionArtifactID, _dynamicallyLoadedDlls, avfIds, startAtRecord);

			return CreateInitializationResults(result);
		}

		public InitializationResults InitializeSearchExport(int appID, int searchArtifactID, int[] avfIds, int startAtRecord)
		{
			_baseServiceContext.AppArtifactID = appID;
			var export = CreateExport((int) ArtifactType.Document);
			CheckExportPermissions(export);
			var result = export.InitializeSavedSearchExport(searchArtifactID, _dynamicallyLoadedDlls, avfIds, startAtRecord);

			return CreateInitializationResults(result);
		}

		public bool HasExportPermissions(int appID)
		{
			_baseServiceContext.AppArtifactID = appID;
			return PermissionsHelper.HasAdminOperationPermission(_baseServiceContext, Permission.AllowDesktopClientExport);
		}

		private InitializationResults CreateInitializationResults(Export.InitializationResults result)
		{
			return new InitializationResults
			{
				ColumnNames = result.ColumnNames,
				RowCount = result.RowCount,
				RunId = result.RunId
			};
		}

		private void CheckExportPermissions(Export export)
		{
			if (!export.HasExportPermissions())
			{
				throw new InsufficientAccessControlListPermissions("Insufficient Permissions! Please ask your Relativity Administrator to allow you export permission.");
			}
		}

		private Export CreateExport(int artifactTypeID, int[] textPrecedenceAvfIds = null)
		{
			Export export;
			if (textPrecedenceAvfIds == null)
			{
				export = new Export(_baseServiceContext, _userPermissionsMatrix, artifactTypeID);
			}
			else
			{
				export = new Export(_baseServiceContext, _userPermissionsMatrix, artifactTypeID, textPrecedenceAvfIds);
			}
			export.SerializeRetrievedDataIntoBytes = false;
			return export;
		}
	}
}