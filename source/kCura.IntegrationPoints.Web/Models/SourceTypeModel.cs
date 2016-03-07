﻿
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

		public SourceProviderConfigModel Config { set; get; }
	}

	[DataContract]
	public class SourceProviderConfigModel
	{
		private readonly SourceProviderConfiguration _originalConfig;
		private readonly Dictionary<Guid, int> _rdoTypeCache;

		public SourceProviderConfigModel(SourceProviderConfiguration originalConfig, Dictionary<Guid, int> rdoTypesCache)
		{
			_originalConfig = originalConfig ?? new SourceProviderConfiguration();
			_rdoTypeCache = rdoTypesCache;
		}

		[DataMember]
		public int[] CompatibleRdoTypes
		{
			get
			{
				if (_originalConfig.CompatibleRdoTypes == null)
				{
					return null;
				}
				List<int> result = new List<int>(_originalConfig.CompatibleRdoTypes.Count);
				foreach (Guid guid in _originalConfig.CompatibleRdoTypes)
				{
					if (_rdoTypeCache.ContainsKey(guid))
					{
						result.Add(_rdoTypeCache[guid]);
					}
				}
				return result.ToArray();
			}
			set // this is requires as part of the data member attribute
			{ }
		}

		[DataMember]
		public ImportSettingVisibility ImportSettingVisibility
		{
			get
			{
				if (_originalConfig.AvailableImportSettings == null)
				{
					_originalConfig.AvailableImportSettings = new ImportSettingVisibility();
				}
				return _originalConfig.AvailableImportSettings;
			}
			set { }
		}
	}
}