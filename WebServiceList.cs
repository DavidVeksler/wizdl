using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace WebServices.UI
{
    public class WebServiceList
    {
        private string _url;
        private WebService[] _services;

        public string Url
        {
            get { return _url; }
        }

        public WebService[] Services
        {
            get { return _services; }
        }

        private WebService FindService(string name)
        {
            foreach (WebService svc in _services)
            {
                if (string.Compare(svc.Name, name, true) == 0)
                {
                    return svc;
                }
            }
            return null;
        }

        public void Serialize(string path)
        {
            XmlTextWriter xml = new XmlTextWriter(path, null);

            xml.Formatting = Formatting.Indented;

            try
            {
                xml.WriteStartElement("services");
                xml.WriteAttributeString("url", _url);

                foreach (WebService svc in _services)
                {
                    xml.WriteStartElement("service");
                    xml.WriteAttributeString("name", svc.Name);

                    foreach (WebMethod method in svc.Methods)
                    {
                        xml.WriteStartElement("method");
                        xml.WriteAttributeString("name", method.Name);
                        new XmlSerializer(method.Arg.GetType()).Serialize(xml, method.Arg);
                        xml.WriteEndElement();
                    }
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();
            }
            finally
            {
                xml.Close();
            }
        }

        public static WebServiceList Deserialize(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            WebServiceList list = LoadFromUrl(doc.DocumentElement.GetAttribute("url"));

            foreach (XmlElement e in doc.GetElementsByTagName("service"))
            {
                WebService svc = list.FindService(e.GetAttribute("name"));

                if (svc == null)
                    continue;

                foreach (XmlElement m in e.GetElementsByTagName("method"))
                {
                    WebMethod method = svc.FindMethod(m.GetAttribute("name"));
                    if (method == null)
                        continue;

                    XmlSerializer ser = new XmlSerializer(method.Arg.GetType());
                    method.SetArg(ser.Deserialize(new StringReader(m.InnerXml)));
                }
            }

            return list;
        }

        public static WebServiceList LoadFromUrl(string url)
        {
            WebServiceList list = new WebServiceList();
            list._url = url;
            list._services = WebService.GetServices(url);
            return list;
        }
    }
}