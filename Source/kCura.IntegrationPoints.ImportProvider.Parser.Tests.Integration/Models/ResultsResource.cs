using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Tests.Integration
{
	[Serializable()]
    [XmlRoot("ResultsResource")]
	public class ResultsResource
	{
		[XmlArrayItem("DataReaderLine")]
		public List<string> DataReaderLines { get; set; }
	}
}
