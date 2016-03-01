
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Web.Models
{
	public class SourceTypeModel
	{
		public string name { get; set; }
		public int id { get; set; }
		public string value { get; set; }
		public string url { get; set; }

		public SourceProviderConfigModel config { set; get; }
	}

	[DataContract]
	public class SourceProviderConfigModel
	{
		private readonly SourceProviderConfiguration _originalConfig;
		private readonly Dictionary<Guid, int> _rdoTypeCache;

		public SourceProviderConfigModel(SourceProviderConfiguration originalConfig, Dictionary<Guid, int> rdoTypesCache)
		{
			_originalConfig = originalConfig;
			_rdoTypeCache = rdoTypesCache;
		}

		[DataMember]
		public int[] CompatibleRdoTypes
		{
			get
			{
				if (_originalConfig == null || _originalConfig.CompartibleRdoTypes == null)
				{
					return null;
				}
				List<int> result = new List<int>(_originalConfig.CompartibleRdoTypes.Count);
				foreach (Guid guid in _originalConfig.CompartibleRdoTypes)
				{
					if (_rdoTypeCache.ContainsKey(guid))
					{
						result.Add(_rdoTypeCache[guid]);
					}
				}
				return result.ToArray();
			}
			set
			{
				
			}
		}
	}
}