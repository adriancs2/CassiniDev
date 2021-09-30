using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;

namespace CassiniDev.Configuration
{
    ///<summary>
    ///</summary>
    public class CassiniDevConfigurationSection : ConfigurationSection
    {

        ///<summary>
        ///</summary>
        public static CassiniDevConfigurationSection Instance
        {
            get
            {
                return (CassiniDevConfigurationSection)ConfigurationManager.GetSection("cassinidev");
            }
        }

        ///<summary>
        ///</summary>
        [ConfigurationProperty("profiles")]
        public CassiniDevProfileElementCollection Profiles
        {
            get
            {
                return (CassiniDevProfileElementCollection)this["profiles"];
            }
        }
    }
}
