using FeedFilter.Core.Models;

namespace FeedFilter.Core;

public interface IXmlParser {
  ParsedXml Parse(string xml);
}
