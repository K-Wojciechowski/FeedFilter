using System.Xml;
using System.Xml.Linq;

namespace FeedFilter.Core.Models;

public record ParsedXml(XDocument Document, XmlNamespaceManager XmlNamespaceManager);
