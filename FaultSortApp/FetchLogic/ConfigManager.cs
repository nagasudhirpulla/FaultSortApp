using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FaultSortApp.FetchLogic
{
    public class ConfigManager
    {
        public int LatencyTime { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string Path { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public void Initialize()
        {
            XmlDocument domDocument = new XmlDocument();

            string path = Environment.CurrentDirectory;
            string xmlPath = path + "\\AppSettings.xml";

            if (File.Exists(xmlPath))
            {
                domDocument.Load(xmlPath);
                XmlNode node = domDocument.SelectSingleNode("HistoryDataProvider/DataSource");

                Host = node.Attributes.GetNamedItem("Host").Value;
                string port = node.Attributes.GetNamedItem("Port").Value;
                Port = Int32.Parse(port);
                Path = node.Attributes.GetNamedItem("Path").Value;
                UserName = node.Attributes.GetNamedItem("UserName").Value;
                Password = node.Attributes.GetNamedItem("Password").Value;

                LatencyTime = 1;

                XmlNode latencyNode = domDocument.SelectSingleNode("HistoryDataProvider/LatencyTime");
                if (latencyNode != null)
                {
                    string latencyValue = latencyNode.Value;
                    int latency = 1;
                    if (int.TryParse(latencyValue, out latency))
                        LatencyTime = latency;
                }
            }
            else
            {
                Host = "172.16.183.131";
                Port = 24721;
                Path = "/eterra-ws/HistoricalTrendProvider";
                UserName = "pdcAdmin";
                Password = "p@ssw0rd";
                LatencyTime = 1;
            }
        }

        internal void Save()
        {
            var sw = new StringWriter(new StringBuilder(1024));
            var xmlTextWriter = new XmlTextWriter(sw);
            xmlTextWriter.Formatting = Formatting.Indented;

            xmlTextWriter.WriteStartElement("HistoryDataProvider");
            xmlTextWriter.WriteStartElement("DataSource");

            xmlTextWriter.WriteStartAttribute("Host");
            xmlTextWriter.WriteString(Host);
            xmlTextWriter.WriteEndAttribute();

            xmlTextWriter.WriteStartAttribute("Port");
            xmlTextWriter.WriteString(this.Port.ToString());
            xmlTextWriter.WriteEndAttribute();

            xmlTextWriter.WriteStartAttribute("Path");
            xmlTextWriter.WriteString(this.Path);
            xmlTextWriter.WriteEndAttribute();

            xmlTextWriter.WriteStartAttribute("UserName");
            xmlTextWriter.WriteString(this.UserName);
            xmlTextWriter.WriteEndAttribute();

            xmlTextWriter.WriteStartAttribute("Password");
            xmlTextWriter.WriteString(this.Password);
            xmlTextWriter.WriteEndAttribute();

            xmlTextWriter.WriteEndElement();

            xmlTextWriter.WriteStartElement("LatencyTime");
            xmlTextWriter.WriteString(LatencyTime.ToString());
            xmlTextWriter.WriteEndElement();

            xmlTextWriter.WriteEndElement();
            xmlTextWriter.Close();

            string path = Environment.CurrentDirectory;
            string xmlPath = path + "\\AppSettings.xml";

            File.WriteAllText(xmlPath, sw.ToString());
        }

    }
}
