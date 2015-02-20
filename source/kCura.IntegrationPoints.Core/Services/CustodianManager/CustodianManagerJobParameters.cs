using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.CustodianManager
{
	[Serializable]
	public class CustodianManagerJobParameters
	{
		public IDictionary<string, string> CustodianManagerMap { get; set; }
		public IEnumerable<FieldMap> CustodianManagerFieldMap { get; set; }
		public bool ManagerFieldIdIsBinary { get; set; }
		public IEnumerable<FieldMap> ManagerFieldMap { get; set; }
	}
}
