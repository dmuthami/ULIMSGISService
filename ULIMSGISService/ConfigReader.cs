using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;//Required to read config files
using System.Collections.Specialized; //NameValueCollection
using System.Collections;

namespace ULIMSGISService
{
    class ConfigReader
    {
        PythonLibrary pythonLibrary;
        public ConfigReader(PythonLibrary pythonLibrary)
        {
            this.pythonLibrary = pythonLibrary;
        }

        public Dictionary<String, String> readNamibiaLocalAuthoritiesSection()
        {
            Dictionary<String, String> dictionary = null;
            var NamibiaLocalAuthorities = ConfigurationManager.GetSection("NamibiaLocalAuthorities") as NameValueCollection;
            if (NamibiaLocalAuthorities != null)
            {
                dictionary = hashtableToDictionary(NamibiaLocalAuthorities);
            }

            return dictionary;
        }
        private Dictionary<String, String> hashtableToDictionary(NameValueCollection nameValueCollection)
        {
            //load items from hashtable to dictionary
            Dictionary<String, String> namibiaTownsDictionary = new Dictionary<String, String>();
            foreach (string key in nameValueCollection)
            {
                namibiaTownsDictionary.Add((String)key, (String)nameValueCollection[key]);
            }
            //return dictionary with towns in it
            return namibiaTownsDictionary;
        }
    }
}
