using System;
using System.Text;
using kCura.EDDS.WebAPI.ExportManagerBase;
using kCura.Relativity.Client;
using kCura.WinEDDS.Service.Export;
using Relativity.Core;
using Relativity.Core.Exception;
using Permission = Relativity.Core.Permission;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
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

		public object[] RetrieveResultsBlock(int appID, Guid runId, int artifactTypeId, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter,
			char nestedValueDelimiter, int[] textPrecedenceAvfIds)
		{
			Export export = CreateExport(appID, artifactTypeId, textPrecedenceAvfIds);
			return export.RetrieveResultsBlock(_baseServiceContext, runId, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter, nestedValueDelimiter);
		}

		public object[] RetrieveResultsBlockStartingFromIndex(int appID, Guid runId, int artifactTypeID, int[] avfIds, int chunkSize,
			bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter, int[] textPrecedenceAvfIds,
			int index)
		{
			Export export = CreateExport(appID, artifactTypeID, textPrecedenceAvfIds);
			return export.RetrieveResultsBlockStartingFromIndex(_baseServiceContext, runId, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter, nestedValueDelimiter, index);
		}

		public object[] RetrieveResultsBlockForProduction(int appID, Guid runId, int artifactTypeId, int[] avfIds, int chunkSize, bool displayMulticodesAsNested,
			char multiValueDelimiter, char nestedValueDelimiter, int[] textPrecedenceAvfIds, int productionId)
		{
			Export export = CreateExport(appID, artifactTypeId, textPrecedenceAvfIds);
			object[] result = export.RetrieveResultsBlockForProduction(_baseServiceContext, runId, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter,
				nestedValueDelimiter, productionId);
			return RehydrateStringsIfNeeded(result);
		}

		public object[] RetrieveResultsBlockForProductionStartingFromIndex(int appID, Guid runId, int artifactTypeID, int[] avfIds,
			int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter,
			int[] textPrecedenceAvfIds, int productionId, int index)
		{
			Export export = CreateExport(appID, artifactTypeID, textPrecedenceAvfIds);
			object[] result = export.RetrieveResultsBlockForProductionStartingFromIndex(_baseServiceContext, runId, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter,
				nestedValueDelimiter, productionId, index);
			return RehydrateStringsIfNeeded(result);
		}

		public InitializationResults InitializeFolderExport(int appId, int viewArtifactId, int parentArtifactId, bool includeSubFolders, int[] avfIds, int startAtRecord,
			int artifactTypeId)
		{
			var export = CreateExportWithPermissionCheck(appId, artifactTypeId);
			return export.InitializeFolderExport(viewArtifactId, parentArtifactId, includeSubFolders, _dynamicallyLoadedDlls, avfIds, startAtRecord).ToInitializationResults();
		}

		public InitializationResults InitializeProductionExport(int appId, int productionArtifactId, int[] avfIds, int startAtRecord)
		{
			var export = CreateExportWithPermissionCheck(appId, (int)ArtifactType.Document);
			return export.InitializeProductionExport(productionArtifactId, _dynamicallyLoadedDlls, avfIds, startAtRecord).ToInitializationResults();
		}

		public InitializationResults InitializeSearchExport(int appID, int searchArtifactID, int[] avfIds, int startAtRecord)
		{
			var export = CreateExportWithPermissionCheck(appID, (int)ArtifactType.Document);
			return export.InitializeSavedSearchExport(searchArtifactID, _dynamicallyLoadedDlls, avfIds, startAtRecord).ToInitializationResults();
		}

		public bool HasExportPermissions(int appID)
		{
			_baseServiceContext.AppArtifactID = appID;
			return PermissionsHelper.HasAdminOperationPermission(_baseServiceContext, Permission.AllowDesktopClientExport);
		}

		private object[] RehydrateStringsIfNeeded(object[] toScrub)
		{
			if (toScrub != null)
			{
				foreach (object[] row in toScrub)
				{
					if (row != null)
					{
						for (int i = 0; i < row.Length; i++)
						{
							if (row[i] is byte[])
							{
								row[i] = Encoding.Unicode.GetString((byte[]) row[i]);
							}
						}
					}
				}
			}
			return toScrub;
		}

		private void CheckExportPermissions(Export export)
		{
			if (!export.HasExportPermissions())
			{
				throw new InsufficientAccessControlListPermissions("Insufficient Permissions! Please ask your Relativity Administrator to allow you export permission.");
			}
		}

		private Export CreateExportWithPermissionCheck(int appId, int artifactTypeId)
		{
			var export = CreateExport(appId, artifactTypeId);
			CheckExportPermissions(export);
			return export;
		}

		private Export CreateExport(int appID, int artifactTypeId, int[] textPrecedenceAvfIds = null)
		{
			_baseServiceContext.AppArtifactID = appID;
			Export export;
			if (textPrecedenceAvfIds == null)
			{
				export = new ExportRIP(_baseServiceContext, _userPermissionsMatrix, artifactTypeId);
			}
			else
			{
				export = new ExportRIP(_baseServiceContext, _userPermissionsMatrix, artifactTypeId, textPrecedenceAvfIds);
			}
			export.SerializeRetrievedDataIntoBytes = false;
			return export;
		}
	}
}