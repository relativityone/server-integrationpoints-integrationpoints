﻿using System;

namespace kCura.IntegrationPoints.SourceProviderInstaller
{
	/// <summary>
	/// Provides the information for the registration of a data source provider.
	/// </summary>
	public class SourceProvider
	{
		/// <summary>
		/// Gets or sets the GUID identifying the data source provider.
		/// </summary>
		internal Guid GUID { get; set; }
		
		internal Guid ApplicationGUID { get; set; }
		internal int ApplicationID { get; set; }
		/// <summary>
		/// Gets or sets the name of the data source provider displayed in the Relativity UI.
		/// </summary>
		public string Name { get; set; }
		
		/// <summary>
		/// Gets or sets the URL used to configure the data source provider.
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// Gets or sets key-value pairs used as source provider settings for display on a custom page.
		/// </summary>
		/// <remarks>
		/// The key-value pairs represent  the names of fields that a user can set on a source provider, and the values that the user has entered for these fields. After a user has created a new integration point, the custom page displays these key-value pairs for reference purposes.
		/// </remarks>
		public string ViewDataUrl { get; set; }
	}
}
