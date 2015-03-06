using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Services.Keywords
{
	public interface IKeyword
	{
		string KeywordName { get; }

		string Convert();

	}
}
