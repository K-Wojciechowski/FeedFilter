using System.Xml.XPath;
using FeedFilter.Core.Enums;
using FeedFilter.Core.Exceptions;
using FeedFilter.Core.Models;
using Microsoft.Extensions.Logging;

namespace FeedFilter.Core;

internal class FilteringEngine(ILogger<FilteringEngine> logger, IXmlParser xmlParser) : IFilteringEngine {
  public FeedFilteringResult Filter(IFeed feed, string xml) {
    var (tree, xmlNamespaceManager) = xmlParser.Parse(xml);
    var entryResults = new List<EntryFilteringResult>();

    // Remove https://en.wikipedia.org/wiki/WebSub links and self links, which could allow feed readers to sidestep our filtering
    foreach (var originalFeedLink in tree.XPathSelectElements("//atom:link[@rel='hub' or @rel='self']",
                 xmlNamespaceManager)) {
      originalFeedLink.Remove();
    }

    foreach (var entryNode in tree.XPathSelectElements("/rss/channel/item|/atom:feed/atom:entry", xmlNamespaceManager)
                 .ToList()) {
      var entry = new Entry(entryNode, xmlNamespaceManager);

      Rule? decidingRule = null;
      IReadOnlyCollection<string>? decidingValues = null;
      foreach (var rule in feed.Rules) {
        var values = GetFieldValuesToMatch(rule, entry);
        if (values.Any(rule.Match)) {
          decidingRule = rule;
          decidingValues = values;
          break;
        }
      }

      var decision = decidingRule?.Decision ?? feed.DefaultDecision;
      entryResults.Add(new EntryFilteringResult(entry.Title, decidingRule, decidingValues, decision));

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

  private IReadOnlyCollection<string> GetFieldValuesToMatch(Rule rule, Entry entry) {
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
      return [];
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
            return [];
          case 1:
            values.Add(attr[0].Value);
            break;
          default:
            logger.LogInformation(
                "Found multiple attributes named '{Attribute}' in entry '{Entry}'. Include the namespace to narrow it down. Ignoring rule {RuleIndex}",
                rule.TestedAttributeName, entry, rule.Index);
            return [];
        }
      }
    }

    return values;
  }
}
