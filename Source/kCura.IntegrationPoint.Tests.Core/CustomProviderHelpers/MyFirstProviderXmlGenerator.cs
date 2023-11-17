using System.Collections.Generic;
using System.Xml;

namespace kCura.IntegrationPoint.Tests.Core.CustomProviderHelpers
{
    public static class MyFirstProviderXmlGenerator
    {
        public static string[] DefaultColumns => new[] { "Name", "Field1", "Field2" };

        public static string[] EntityColumns => new[] { "UniqueID", "LastName", "FirstName", "Manager" };

        public static string GenerateRecords(int recordCount, string[] fields = null)
        {
            XmlDocument doc = new XmlDocument();

            string[] columns = fields is null ? DefaultColumns : fields;

            XmlElement allColumnsNode = doc.CreateElement("columns");
            foreach (string column in columns)
            {
                XmlElement columnNode = doc.CreateElement("column");
                columnNode.InnerText = column;
                allColumnsNode.AppendChild(columnNode);
            }

            XmlElement dataNode = doc.CreateElement("data");
            foreach (XmlNode row in GetRows(recordCount, columns, doc))
            {
                dataNode.AppendChild(row);
            }

            XmlElement rootNode = doc.CreateElement("root");
            rootNode.AppendChild(allColumnsNode);
            rootNode.AppendChild(dataNode);

            doc.AppendChild(rootNode);

            return doc.OuterXml;
        }

        public static string GenerateEntitiesRecords(int recordCount)
        {
            return GenerateRecords(recordCount, EntityColumns);
        }

        private static IEnumerable<XmlNode> GetRows(int recordCount, string[] columns, XmlDocument xmlDocument)
        {
            for (int i = 0; i < recordCount; i++)
            {
                XmlElement documentNode = xmlDocument.CreateElement("document");
                foreach (string column in columns)
                {
                    XmlElement columnNode = xmlDocument.CreateElement(column);
                    columnNode.InnerText = i.ToString();
                    documentNode.AppendChild(columnNode);
                }

                yield return documentNode;
            }
        }
    }
}
