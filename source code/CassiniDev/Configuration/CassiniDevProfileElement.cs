using System;
using System.Configuration;

namespace CassiniDev.Configuration
{
    [ConfigurationCollection(typeof(MimeTypeElement))]
    public class MimeTypeElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MimeTypeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MimeTypeElement)element).Extension;
        }
    }
    public class MimeTypeElement : ConfigurationElement
    {
        [ConfigurationProperty("extension", IsRequired = true, IsKey = true)]
        public string Extension
        {
            get
            {
                return (string)this["extension"];
            }
            set
            {
                this["extension"] = value;
            }
        }
        [ConfigurationProperty("mimetype", IsRequired = true)]
        public string MimeType
        {
            get
            {
                return (string)this["mimetype"];
            }
            set
            {
                this["mimetype"] = value;
            }
        }


    }

    ///<summary>
    ///</summary>
    public class CassiniDevProfileElement : ConfigurationElement
    {
        /// <summary>
        /// Port is used as profile selector
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = "*", IsKey = true, IsRequired = true)]
        public string Port
        {
            get
            {
                return (string)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        ///<summary>
        ///</summary>
        [ConfigurationProperty("path")]
        public string Path
        {
            get
            {
                return (string)this["path"];
            }
            set
            {
                this["path"] = value;
            }
        }




        ///<summary>
        ///</summary>
        [ConfigurationProperty("hostName")]
        public string HostName
        {
            get
            {
                return (string)this["hostName"];
            }
            set
            {
                this["hostName"] = value;
            }
        }

        ///<summary>
        ///</summary>
        [ConfigurationProperty("ip")]
        public string IpAddress
        {
            get
            {
                return (string)this["ip"];
            }
            set
            {
                this["ip"] = value;
            }
        }

        ///<summary>
        ///</summary>
        [ConfigurationProperty("ipMode", DefaultValue = CassiniDev.IPMode.Loopback)]
        public IPMode IpMode
        {
            get
            {
                return (IPMode)this["ipMode"];
            }
            set
            {
                this["ipMode"] = value;
            }
        }

        ///<summary>
        ///</summary>
        [ConfigurationProperty("v6", DefaultValue = false)]
        public bool IpV6
        {
            get
            {
                return (bool)this["v6"];
            }
            set
            {
                this["v6"] = value;
            }
        }


        ///<summary>
        ///</summary>
        [ConfigurationProperty("plugins")]
        public PluginElementCollection Plugins
        {
            get
            {
                return (PluginElementCollection)this["plugins"];
            }
        }


        ///<summary>
        ///</summary>
        [ConfigurationProperty("mimeTypes")]
        public MimeTypeElementCollection MimeTypes
        {
            get
            {
                return (MimeTypeElementCollection)this["mimeTypes"];
            }
        }
    }
}