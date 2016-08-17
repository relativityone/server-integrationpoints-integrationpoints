﻿using System;
using System.Linq;
using kCura.IntegrationPoints.Config;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	internal class ConfigFactory : IConfigFactory
	{
		public IConfig Create()
		{
			// we are relying here on the assumption that Config object will have
			// all of its properties properly set at the moment of usage
			// (e.g. Manager.Settings.Factory) - swolny

			return Config.Config.Instance;
		}
	}
}