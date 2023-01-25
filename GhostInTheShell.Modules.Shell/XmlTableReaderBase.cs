using System;
using System.IO;
using System.Xml;

namespace GhostInTheShell.Modules.Shell
{
    public abstract class XmlTableReaderBase
    {
        /// <exception cref="ArgumentNullException"></exception>
        protected XmlReader CreateTableReader(Stream tableStream)
        {
            if (tableStream is null)
                throw new ArgumentNullException(nameof(tableStream));

            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreComments = true;

            return XmlReader.Create(tableStream, xmlReaderSettings);
        }
    }
}