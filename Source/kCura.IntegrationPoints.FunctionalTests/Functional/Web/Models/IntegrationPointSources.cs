using Atata;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Models
{
	public enum IntegrationPointSources
	{
		[Term("Select...")]
		Select,
		[Term("FTP (CSV File)")]
		FTP,
		[Term("LDAP")]
		LDAP,
		[Term("Load File")]
		LoadFile
	}
}
