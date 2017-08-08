using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using kCura.WinEDDS.Service;
using Relativity.API;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class RipUserManger : UserManager
	{
		public RipUserManger(ICredentials credentials, CookieContainer cookieContainer) : base(credentials, cookieContainer)
		{
		}

		public RipUserManger(ICredentials credentials, CookieContainer cookieContainer, string webServiceUrl) : base(credentials, cookieContainer)
		{
			this.Url = string.Format("{0}UserManager.asmx", webServiceUrl);
		}

		public static NetworkCredential LoginUsernamePassword(string username, string password, CookieContainer cookieContainer,
			string webServiceUrl)
		{
			var networkCred = new NetworkCredential(username, password);
			var userMgr = new RipUserManger(networkCred, cookieContainer, webServiceUrl);

			if (userMgr.Login(username, password))
			{
				return networkCred;
			}
			return null;
		}
	}

	public class RipFolderManager : FolderManager
	{
		public RipFolderManager(ICredentials credentials, CookieContainer cookieContainer, string url) : base(credentials, cookieContainer)
		{
			this.Url = string.Format("{0}FolderManager.asmx", url);
		}
	}

	public class RipCaseManager : CaseManager
	{
		public RipCaseManager(ICredentials credentials, CookieContainer cookieContainer, string url) : base(credentials, cookieContainer)
		{
			this.Url = string.Format("{0}CaseManager.asmx", url);
		}
	}

	public interface IFolderManagerService
	{
		IEnumerable<Artifact> GetFolders(int caseId);
	}

	public class FolderManagerService : IFolderManagerService
	{
		private readonly RipFolderManager _folderManager;
		private readonly RipCaseManager _caseManager;

		public FolderManagerService(CookieContainer cookieContainer, NetworkCredential networkCredential, string webServiceUrl)
		{
			_folderManager = new RipFolderManager(networkCredential, cookieContainer, webServiceUrl);
			_caseManager = new RipCaseManager(networkCredential, cookieContainer, webServiceUrl);
		}

		public IEnumerable<Artifact> GetFolders(int caseId)
		{
			var wksp = _caseManager.Read(caseId);
			var ds = _folderManager.RetrieveFolderAndDescendants(caseId, wksp.RootFolderID);
			foreach (DataRow dataRow in ds.Tables[0].Rows)
			{
				yield return new Artifact
				{
					ArtifactID = Convert.ToInt32(dataRow[0]),
					Name = dataRow[1].ToString(),
					ParentArtifactID = Utility.NullableTypesHelper.DBNullConvertToNullable<int>(dataRow[2])
				};
			}
		}
	}

	public class ArtifactTreeCreator : IArtifactTreeCreator
	{
		private readonly IAPILog _logger;

		public ArtifactTreeCreator(IHelper helper)
		{
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<IHelper>();
		}

		public JsTreeItemDTO Create(IEnumerable<Artifact> nodes)
		{
			var treeItemsFlat = ConvertToTreeItems(nodes);
			var root = FindRoot(treeItemsFlat);
			root.Icon = JsTreeItemIconEnum.Root.GetDescription();

			BuildTree(treeItemsFlat);

			return root;
		}

		private IList<JsTreeItemWithParentIdDTO> ConvertToTreeItems(IEnumerable<Artifact> nodes)
		{
			return nodes.Select(x => x.ToTreeItemWithParentIdDTO()).OrderBy(x => x.Text).ToList();
		}

		private JsTreeItemDTO FindRoot(IList<JsTreeItemWithParentIdDTO> treeItemsFlat)
		{
			var ids = treeItemsFlat.Select(x => x.Id).ToList();
			var root = treeItemsFlat.Where(x => string.IsNullOrEmpty(x.ParentId) || !ids.Contains(x.ParentId)).ToList();

			if (root.Count != 1)
			{
				LogMissingRootError();
				throw new NotFoundException("Root not found");
			}

			return root[0];
		}

		private void BuildTree(IList<JsTreeItemWithParentIdDTO> treeItemsFlat)
		{
			foreach (var treeItem in treeItemsFlat)
			{
				var children = treeItemsFlat.Where(ch => ch.ParentId == treeItem.Id);
				treeItem.Children.AddRange(children);
			}
		}

		#region Logging

		private void LogMissingRootError()
		{
			_logger.LogError("Root for tree not found.");
		}

		#endregion
	}
}