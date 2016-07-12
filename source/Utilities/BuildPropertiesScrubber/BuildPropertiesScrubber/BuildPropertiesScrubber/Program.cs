using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using BuildPropertiesScrubber.Models;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace BuildPropertiesScrubber
{
	public class Program
	{
		public static void Main(string[] args)
		{
			string projLocation = args[0];
			string nugetPackage = args[1];

			Packages nugetReferences = GetNugetPackages(nugetPackage);

			Project project = new Project(projLocation);
			ProjectPropertyGroupElement propertyGroup = GetPropertyGroupElement(project, "ExtendedDependencies");
			HashSet<string> existingProperties = new HashSet<string>(propertyGroup.Properties.Select(x => x.Name), StringComparer.OrdinalIgnoreCase);

			foreach (Package reference in nugetReferences.Items)
			{
				string sanatizedId = reference.Id.Replace(".", "_");
				string value = $"{reference.Id}.{reference.Version}";
				if (existingProperties.Contains(sanatizedId))
				{
					propertyGroup.SetProperty(sanatizedId, value);
				}
				else
				{
					existingProperties.Add(sanatizedId);
					propertyGroup.AddProperty(sanatizedId, value);
				}
			}
			project.Save();
		}

		private static ProjectPropertyGroupElement GetPropertyGroupElement(Project project, string name)
		{
			var root = project.Xml;
			foreach (ProjectElement child in root.Children)
			{
				if (child.Label == name)
				{
					ProjectPropertyGroupElement propertyGroup = (ProjectPropertyGroupElement)child;
					return propertyGroup;
				}
			}
			return null;
		}

		private static Packages GetNugetPackages(string nugetPackage)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(Packages));
			using (FileStream fs = new FileStream(nugetPackage, FileMode.Open))
			{
				using (XmlReader reader = XmlReader.Create(fs))
				{
					Packages packages = (Packages)serializer.Deserialize(reader);
					return packages;
				}
			}
		}
	}
}