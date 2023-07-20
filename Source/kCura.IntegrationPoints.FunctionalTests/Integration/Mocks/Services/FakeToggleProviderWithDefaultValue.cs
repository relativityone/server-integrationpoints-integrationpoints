﻿using Relativity.Toggles;
using System.Reflection;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.Services
{
	public class FakeToggleProviderWithDefaultValue : IToggleProvider
	{
		public bool IsEnabled<T>() where T : IToggle
		{
			DefaultValueAttribute attribute = typeof(T).GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;
			return attribute.Value;
		}

		public Task<bool> IsEnabledAsync<T>() where T : IToggle
		{
			throw new System.NotImplementedException();
		}

		public bool IsEnabledByGuid(System.Guid componentGuid)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> IsEnabledByGuidAsync(System.Guid componentGuid)
		{
			throw new System.NotImplementedException();
		}

		public bool IsEnabledByName(string toggleName)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> IsEnabledByNameAsync(string toggleName)
		{
			throw new System.NotImplementedException();
		}

		public Task SetAsync<T>(bool enabled) where T : IToggle
		{
			throw new System.NotImplementedException();
		}

		public MissingFeatureBehavior DefaultMissingFeatureBehavior { get; }
		public bool CacheEnabled { get; set; }
		public int CacheTimeoutInSeconds { get; set; }
	}
}