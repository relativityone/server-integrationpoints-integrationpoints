using System;
using kCura.IntegrationPoints.Core;

namespace kCura.IntegrationPoints.EventHandlers
{
	public class ExternalTabURLService
	{
		private static System.Guid ReportGuid = Guid.Parse("6C36149D-DFA9-4EB3-872B-D14FEFCBF3A1");

		private readonly IServiceContext _context;
		public ExternalTabURLService()
		{
			//_context = context;
		}
		
		//public virtual string GenerateNoneEncryptedURL(Guid tabGuid, string externalLink, bool useStandards = false)
		//{
		//	var tabID = ExternalTabCache.GetTabID(_context.SqlContext, tabGuid);
		//	string returnLink = GenerateURLWithNoEncoding(externalLink, useStandards);
		//	var encodedLink = EncodeRelativityURL(returnLink, _context.WorkSpaceId, tabID, false);
		//	return encodedLink;
		//}

		//public virtual string generateurl(string tabguid, string externallink, bool usestandards = false)
		//{
		////	var tabid = externaltabcache.gettabid(_context.sqlservice, tabguid);
		//	string returnlink = generateurlwithnoencoding(externallink, usestandards);
		//	var encodedlink = encoderelativityurl(returnlink, _context.workspaceid, tabid, true);
		//	return encodedlink;
		//}

		private string GenerateURLWithNoEncoding(string externalLink, bool useStandards = false)
		{
			string returnLink = externalLink;
			if (useStandards)
			{
				if (returnLink.Contains("?"))
					returnLink = returnLink + "&StandardsCompliance=true";
				else
					returnLink = returnLink + "?StandardsCompliance=true";
			}
			return returnLink;
		}

		public string EncodeRelativityURL(string externalLink, int workspaceID, int tabID, bool encryptLink = true)
		{
			var encryptedExternalLink = externalLink;
			//if (encryptLink) encryptedExternalLink = Core.Crypto.TripleDESEncryptor.Encrypt(encryptedExternalLink, Core.Constants.PartialURLS.EXTERNAL_HREF_ENCRYPTION_KEY);
			var directTo = encryptedExternalLink;//System.Web.HttpUtility.UrlEncode(encryptedExternalLink);
			var qlLink = encryptLink ? string.Format("/Relativity/External.aspx?AppID={0}&ArtifactID={0}&DirectTo={1}&SelectedTab={2}", workspaceID, directTo, tabID) : string.Format("/Relativity/External.aspx?AppID={0}&ArtifactID={0}&DirectTo={1}&SelectedTab={2}", workspaceID, directTo, tabID);
			return qlLink;
		}

		
		public static string Base64Encode(string plainText)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return System.Convert.ToBase64String(plainTextBytes);
		}

		public static string Base64Decode(string base64EncodedData)
		{
			var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}
	}
}
