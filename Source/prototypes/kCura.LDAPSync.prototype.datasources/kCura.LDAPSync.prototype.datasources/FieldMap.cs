﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.LDAPSync.prototype.datasources
{
	/// <summary>
	/// Responsible for where the data is coming from to where the data is going to
	/// </summary>
	public class FieldMap
	{
		/// <summary>
		/// The field where the data is coming from
		/// </summary>
		public FieldEntry SourceField { get; set; }
		/// <summary>
		/// The field where the data should be going to
		/// </summary>
		public FieldEntry DestinationField { get; set; }

		public bool IsIDField { get; set; }
	}
}