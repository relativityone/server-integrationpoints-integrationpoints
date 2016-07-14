using System;
using System.Xml;
using System.Xml.Serialization;

namespace BuildPropertiesScrubber.Models
{
	[Serializable]
	public class Package
	{
		[XmlAttribute("id")]
		public string Id { get; set; }

		[XmlAttribute("version")]
		public string Version { get; set; }

		[XmlAttribute()]
		public string TargetFramework { get; set; }
	}
}