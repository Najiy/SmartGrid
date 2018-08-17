using System;
using System.Collections.Generic;
using System.Xml;

namespace SmartGrid
{
    public static class Extensions
    {
        public static List<XmlNode> Where(this XmlNodeList self, Predicate<XmlNode> predicate)
        {
            List<XmlNode> newlist = new List<XmlNode>();

            for (int i = 0; i < self.Count; i++)
            {
                if (predicate.Invoke(self[i]))
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
                if (predicate.Invoke(self[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool All(this XmlNodeList self, Predicate<XmlNode> predicate)
        {
            for (int i = 0; i < self.Count; i++)
            {
                if (!(predicate.Invoke(self[i])))
                {
                    return false;
                }
            }
            return true;
        }

        public static List<XmlNode> Skip(this XmlNodeList self, int numSkips)
        {
            List<XmlNode> newlist = new List<XmlNode>();
            for (int i = 0; i < self.Count; i++)
            {
                if (i >= numSkips)
                {
                    newlist.Add(self.Item(i));
                }
            }
            return newlist;
        }

        public static List<XmlNode> Take(this XmlNodeList self, int numSkips)
        {
            List<XmlNode> newlist = new List<XmlNode>();
            for (int i = 0; i < self.Count; i++)
            {
                if (i <= numSkips)
                {
                    newlist.Add(self.Item(i));
                }
            }
            return newlist;
        }
    }
}