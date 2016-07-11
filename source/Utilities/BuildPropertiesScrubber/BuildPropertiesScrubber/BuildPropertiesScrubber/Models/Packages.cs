using System;
using System.Xml;
using System.Xml.Serialization;

namespace BuildPropertiesScrubber.Models
{
	[XmlRoot("packages"), Serializable]
	public class Packages
	{
		[XmlElement("package")]
		public Package[] Items;
	}
}