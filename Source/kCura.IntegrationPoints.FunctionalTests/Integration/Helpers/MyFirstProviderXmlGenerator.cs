using System.Collections.Generic;
using System.Xml;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
    public class MyFirstProviderXmlGenerator
    {
        public string GenerateRecords(int recordCount)
        {
            XmlDocument doc = new XmlDocument();
            string[] columns = { "Name", "Field1", "Field2" };

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

        public string GenerateEntitiesRecords(int recordCount)
        {
            XmlDocument doc = new XmlDocument();
            string[] columns = { "UniqueID", "LastName", "FirstName", "Manager"};

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

        private IEnumerable<XmlNode> GetRows(int recordCount, string[] columns, XmlDocument xmlDocument)
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
