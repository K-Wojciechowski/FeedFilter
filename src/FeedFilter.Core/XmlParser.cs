using System.Xml;
using System.Xml.Linq;
using FeedFilter.Core.Models;

namespace FeedFilter.Core;

internal class XmlParser : IXmlParser {
  public ParsedXml Parse(string xml) {
    var reader = new XmlTextReader(xml, XmlNodeType.Document, null);
    var document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);

    var manager = new XmlNamespaceManager(reader.NameTable);
    manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
    manager.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");
    manager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

    return new ParsedXml(document, manager);
  }
}
