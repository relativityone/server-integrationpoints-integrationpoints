using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Process
{

	public class ExportStatistics : Statistics
	{
		public override IDictionary ToDictionary()
		{
			System.Collections.Specialized.HybridDictionary retval = new System.Collections.Specialized.HybridDictionary();
			if (!(this.FileTime == 0))
				retval.Add("Average file transfer rate", ToFileSizeSpecification(this.FileBytes / ((double)this.FileTime / 10000000)) + "/sec");
			if (!(this.MetadataTime == 0) && !(this.MetadataBytes == 0))
				retval.Add("Average metadata transfer rate (includes SQL processing)", ToFileSizeSpecification(this.MetadataBytes / ((double)this.MetadataTime / 10000000)) + "/sec");
			return retval;
		}
	}
}
