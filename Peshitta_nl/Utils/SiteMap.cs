using System;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;
using System.Text;
namespace adccure
{
    public enum EnumChangeFreq
    {
        notset,
        always,
        hourly,
        daily,
        weekly,
        monthly,
        yearly,
        never
    }

    [XmlRoot(ElementName = "urlset", Namespace = SCHEMA_SITEMAP)]
    public sealed class UrlSet
    {
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns;
        private const string XSI_NAMESPACE = "http://www.w3.org/2001/XMLSchema-instance";
        private const string SCHEMA_SITEMAP = "http://www.sitemaps.org/schemas/sitemap/0.9";


        private Url[] _url;

        public UrlSet()
        {
            _url = new Url[0];
            xmlns = new XmlSerializerNamespaces();
            xmlns.Add("xsi", XSI_NAMESPACE);
            SchemaLocation = SCHEMA_SITEMAP + " " + SCHEMA_SITEMAP + "/sitemap.xsd";

        }
        [XmlAttribute(AttributeName = "schemaLocation", Namespace = XSI_NAMESPACE)]
        public string SchemaLocation;

        public void AddUrl(Url url)
        {
            int l = _url.Length + 1;
            Array.Resize(ref _url, l);
            _url[l - 1] = url;
        }

        [XmlElement(ElementName = "url")]
        public Url[] url
        {
            get { return _url; }
            set { _url = value; } //bogus
        }
        /// <summary>
        /// serializes the UrlSet to a sitemap.xsd conform string ready for saving to disk.
        /// </summary>
        /// <returns>a Stream object</returns>
        public Stream ToStream()
        {
            XmlSerializer xmlser = new XmlSerializer(GetType());
            var io = new MemoryStream();
            StreamWriter sw = new StreamWriter(io, Encoding.UTF8);            
            
            xmlser.Serialize(sw, this);

             sw.Flush();
           io.Position = 0;
            return io;
        }
    }

    public sealed class Url
    {
        private string _loc;
        private DateTime _lastmod;
        private float _priority;
        private EnumChangeFreq _changefreq;

        public Url()
        {
            //setting defaults
            _changefreq = EnumChangeFreq.notset;
            _priority = 0.0F;
            _lastmod = DateTime.MinValue;
        }



        [XmlElement(ElementName = "loc")]
        public string Loc
        {
            get
            {
                return _loc;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException();
                }
                if (value.Length < 12 || value.Length > 2048)
                {
                    throw new ArgumentException("loc must be between 12 and 2048 in length");
                }
                _loc = value;
            }
        }
        [XmlElement(ElementName = "lastmod"), DefaultValue(typeof(DateTime), "1-1-0001")]
        public DateTime LastModifiedDateTime
        {
            get
            {
                return _lastmod;
            }

            set
            {
                _lastmod = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind);

            }
        }
        [XmlElement(ElementName = "changefreq")]
        [DefaultValue(EnumChangeFreq.notset)]
        public EnumChangeFreq ChangeFreq
        {
            get
            {
                return _changefreq;
            }

            set
            {
                _changefreq = value;
            }
        }
        [XmlElement(ElementName = "priority")]
        [DefaultValue(0.0F)]
        public float Priority
        {
            get
            {
                return _priority;
            }

            set
            {
                if (value < 0 || value > 1.0)
                {
                    throw new ArgumentException("Must be between 0 and 1");
                }
                _priority = value;
            }
        }
    }
}