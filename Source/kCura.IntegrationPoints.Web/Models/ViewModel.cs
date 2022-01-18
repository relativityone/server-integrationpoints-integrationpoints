using Relativity.Services.Objects.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Web.Models
{
	public class ViewModel
	{
		public ViewModel(RelativityObject relativityObject)
		{
			DisplayName = relativityObject.Name;
			Value = relativityObject.ArtifactID;
		}

		public ViewModel()
		{
		}

		public int Value { get; set; }
		public string DisplayName { get; set; }
	}
}
