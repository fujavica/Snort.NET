using System;
using snortdb;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace razor
{
    public class MyConfig
    {
        public string rfcUrl { get; set; }
    }

    public static class XmlUtils
    {
        static XmlDocument xdoc = new XmlDocument();
        static string path = System.AppDomain.CurrentDomain.BaseDirectory;
        static string rfc_ref = Startup.Configuration.GetSection("Sources:rfcUrl").Value;
        public static List<TransportProtocol> GetTransportProcotols()
        {
            List<TransportProtocol> protocols = new List<TransportProtocol>();
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("wwwroot/service-names-port-numbers.xml");
            XmlNode root = xdoc.DocumentElement;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr.AddNamespace("i", "http://www.iana.org/assignments");
            int value;

            foreach (XmlNode record in root.SelectNodes(@"//i:record", nsmgr))
            {
                if (int.TryParse(record.SelectSingleNode("i:number", nsmgr)?.InnerText, out value))
                {
                    TransportProtocol p = new TransportProtocol(value, record.SelectSingleNode("i:protocol", nsmgr)?.InnerText,
                                       value.ToString() + " (" + record.SelectSingleNode("i:name", nsmgr)?.InnerText + ")"
                                       + " – " + record.SelectSingleNode("i:description", nsmgr)?.InnerText
                                      );
                    if (record.SelectSingleNode(@"i:xref[@type='rfc']", nsmgr) != null)
                        p.xref = rfc_ref + record.SelectSingleNode(@"i:xref[@type='rfc']", nsmgr)?.Attributes["data"]?.InnerText;

                    protocols.Add(p);
                }
            }

            return protocols;
        }

        public static List<Protocol> GetProcotols()
        {
            List<Protocol> protocols = new List<Protocol>();
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load("wwwroot/protocol-numbers.xml");
            XmlNode root = xdoc.DocumentElement;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr.AddNamespace("i", "http://www.iana.org/assignments");
            int value;
            foreach (XmlNode record in root.SelectNodes(@"i:registry/i:record", nsmgr))
            {
                if (int.TryParse(record.SelectSingleNode("i:value", nsmgr)?.InnerText, out value))
                {
                    Protocol p = new Protocol(value,
                                       value.ToString() + " (" + record.SelectSingleNode("i:name", nsmgr)?.InnerText + ")",
                                       "" + record.SelectSingleNode("i:name", nsmgr)?.InnerText
                                      //+ " – " + record.SelectSingleNode("i:description", nsmgr)?.InnerText
                                      );
                    if (record.SelectSingleNode(@"i:xref", nsmgr)?.Attributes["type"]?.InnerText == "rfc")
                    {
                        p.reference = rfc_ref + record.SelectSingleNode(@"i:xref", nsmgr)?.Attributes["data"]?.InnerText;
                    }
                    protocols.Add(p);
                }
            }

            return protocols;
        }


    }
}