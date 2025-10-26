using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FeedFilter.Core.Models;

public record Entry(XElement Element, XmlNamespaceManager XmlNamespaceManager) {
  public XElement? TitleNode => Element.XPathSelectElement("*[local-name()='title']");
  public string? Title => TitleNode?.Value;

  public IEnumerable<XElement> XPathSelectElements(string expression) =>
      Element.XPathSelectElements(expression, XmlNamespaceManager);

  public override string ToString() => Title ?? Element.ToString();

  public void Remove() => Element.Remove();
}
