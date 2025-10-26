using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FeedFilter.Core.Models;

public record ParsedXml(XDocument Document, XmlNamespaceManager XmlNamespaceManager);
