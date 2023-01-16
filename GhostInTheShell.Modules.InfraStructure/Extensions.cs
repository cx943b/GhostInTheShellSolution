using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GhostInTheShell.Modules.InfraStructure
{
    public static class Extensions
    {
        public static IDictionary<string, string> ToDictionary(this XmlAttributeCollection attCollection)
        {
            return attCollection.Cast<XmlAttribute>()
                .Select(att => new KeyValuePair<string, string>(att.Name, att.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
