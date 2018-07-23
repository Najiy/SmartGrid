using System;
using System.Collections.Generic;
using System.Xml;

namespace OsmSmartGrid
{
    public static class Extensions
    {
        public static List<XmlNode> Where(this XmlNodeList self, Predicate<XmlNode> predicate)
        {
            List<XmlNode> newlist = new List<XmlNode>();

            for (int i = 0; i < self.Count; i++)
            {
                if (predicate.Invoke(self[i]) == true)
                {
                    newlist.Add(self[i]);
                }
            }

            return newlist;
        }

        public static bool Has(this XmlNodeList self, Predicate<XmlNode> predicate)
        {
            List<XmlNode> newlist = new List<XmlNode>();

            for (int i = 0; i < self.Count; i++)
            {
                if (predicate.Invoke(self[i]) == true)
                {
                    return true;
                }
            }

            return false;
        }

    }
}