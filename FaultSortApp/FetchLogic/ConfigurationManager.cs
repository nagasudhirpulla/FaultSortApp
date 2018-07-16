using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace FaultSortApp
{
    public class ConfigurationManager : DependencyObject
    {
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
                if(latencyNode != null)
                {
                    string latencyValue = latencyNode.Value;
                    int latency = 1;
                    if(int.TryParse(latencyValue, out latency))
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

        public int LatencyTime
        {
            get;
            set;
        }

        public string Host
        {
            get { return (string)GetValue(HostProperty); }
            set { SetValue(HostProperty, value); }
        }
        public static readonly DependencyProperty HostProperty = DependencyProperty.Register("Host", typeof(string), typeof(ConfigurationManager), new PropertyMetadata("10.165.195.53"));

        public int Port
        {
            get { return (int)GetValue(PortProperty); }
            set { SetValue(PortProperty, value); }
        }
        public static readonly DependencyProperty PortProperty = DependencyProperty.Register("Port", typeof(int), typeof(ConfigurationManager), new PropertyMetadata(24721));

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(ConfigurationManager), new PropertyMetadata("/eterra-ws/HistoricalTrendProvider"));

        public string UserName
        {
            get { return (string)GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }
        public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register("UserName", typeof(string), typeof(ConfigurationManager), new PropertyMetadata("pdcAdmin"));

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password", typeof(string), typeof(ConfigurationManager), new PropertyMetadata("p@ssw0rd"));

        internal void Save()
        {
            var sw = new StringWriter(new StringBuilder(1024));
            var xmlTextWriter = new XmlTextWriter(sw);
            xmlTextWriter.Formatting = Formatting.Indented;

            xmlTextWriter.WriteStartElement("HistoryDataProvider");
            xmlTextWriter.WriteStartElement("DataSource");

            xmlTextWriter.WriteStartAttribute("Host");
            xmlTextWriter.WriteString(this.Host);
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
