using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using FeedFilter.Core.Enums;
using FeedFilter.Core.Exceptions;
using FeedFilter.Core.Models;
using Microsoft.Extensions.Logging;

namespace FeedFilter.Core;

public class FilteringEngine(ILogger<FilteringEngine> logger) : IFilteringEngine {
  public FeedFilteringResult Filter(IFeed feed, string xml) {
    var reader = new XmlTextReader(xml, XmlNodeType.Document, null);
    var tree = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
    var xmlNamespaceManager = GetXmlNamespaceManager(reader);
    var entryResults = new List<EntryFilteringResult>();

    // Remove https://en.wikipedia.org/wiki/WebSub links and self links, which could allow feed readers to sidestep our filtering
    foreach (var originalFeedLink in tree.XPathSelectElements("//atom:link[@rel='hub' or @rel='self']",
                 xmlNamespaceManager)) {
      originalFeedLink.Remove();
    }

    foreach (var entryNode in tree.XPathSelectElements("/rss/channel/item|/atom:feed/atom:entry", xmlNamespaceManager)
                 .ToList()) {
      var entry = new Entry(entryNode, xmlNamespaceManager);
      var decidingRule = feed.Rules.FirstOrDefault(rule => TestRule(rule, entry));
      var decision = decidingRule?.Decision ?? feed.DefaultDecision;
      entryResults.Add(new EntryFilteringResult(entry.Title, decidingRule, decision));

      switch (decision) {
        case Decision.Accept:
          break;
        case Decision.Reject:
          entry.Remove();
          break;
        case Decision.Promote:
          entry.TitleNode?.AddFirst(PromoteDemoteIcons.PromoteIcon);
          break;
        case Decision.Demote:
          entry.TitleNode?.AddFirst(PromoteDemoteIcons.DemoteIcon);
          break;
        default:
          throw new InvalidOperationException("Unknown decision");
      }
    }


    var filteredXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + tree.ToString().ReplaceLineEndings("\n");

    return new FeedFilteringResult(feed, xml, filteredXml, entryResults);
  }

  private static XmlNamespaceManager GetXmlNamespaceManager(XmlTextReader reader) {
    var manager = new XmlNamespaceManager(reader.NameTable);
    manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
    manager.AddNamespace("content", "http://purl.org/rss/1.0/modules/content/");
    manager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
    return manager;
  }

  private bool TestRule(Rule rule, Entry entry) {
    string[] xpaths = rule.Field switch {
        ItemField.Title => ["atom:title", "title"],
        ItemField.Author => ["atom:author/atom:name", "atom:author", "dc:creator", "author"],
        ItemField.Link => ["atom:link[@rel='alternate']", "link"],
        ItemField.Category => ["atom:category", "category"],
        ItemField.Content =>
            ["content:encoded", "atom:content[@type='xhtml']", "atom:content[@type='html']", "description"],
        ItemField.Custom when !string.IsNullOrEmpty(rule.CustomXPath) => [rule.CustomXPath],
        ItemField.Custom => throw new InvalidRuleException("Missing custom XPath"),
        _ => throw new InvalidRuleException("Unexpected ItemField")
    };

    var elements = xpaths.SelectMany(entry.XPathSelectElements).ToList();
    if (elements.Count == 0) {
      logger.LogInformation(
          "Unable to find '{Field}' in entry '{Entry}'. Tested XPaths: '{XPaths}'. Ignoring rule {RuleIndex}",
          rule.Field,
          entry, string.Join("', '", xpaths), rule.Index);
      return false;
    }

    var values = new List<string>();

    foreach (var element in elements) {
      if (rule.Field == ItemField.Link && element.Attribute("href") is { } hrefAttribute) {
        values.Add(hrefAttribute.Value);
      } else if (rule.TestedAttributeName == null) {
        values.Add(element.Value);
      } else {
        var fullAttr = element.Attributes()
            .Where(attr => attr.Name.ToString() == rule.TestedAttributeName)
            .ToArray();
        var attr = fullAttr.Length > 0
            ? fullAttr
            : element.Attributes()
                .Where(attr =>
                    attr.Name.LocalName == rule.TestedAttributeName || attr.Name.ToString() == rule.TestedAttributeName)
                .ToArray();
        switch (attr.Length) {
          case 0:
            logger.LogInformation(
                "Unable to find attribute '{Attribute}' in entry '{Entry}'. Ignoring rule {RuleIndex}",
                rule.TestedAttributeName, entry, rule.Index);
            return false;
          case 1:
            values.Add(attr[0].Value);
            break;
          default:
            logger.LogInformation(
                "Found multiple attributes named '{Attribute}' in entry '{Entry}'. Include the namespace to narrow it down. Ignoring rule {RuleIndex}",
                rule.TestedAttributeName, entry, rule.Index);
            return false;
        }
      }
    }

    return values.Any(rule.Match);
  }
}
