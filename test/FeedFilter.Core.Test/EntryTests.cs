using System.Xml.XPath;
using FeedFilter.Core.Models;
using Shouldly;

namespace FeedFilter.Core.Test;

[TestClass]
public sealed class EntryTests {
  [TestMethod]
  public void TitleNode_RssEntry_ReturnsNodeNamedTitle() {
    // Arrange
    var (tree, xmlNamespaceManager) = new XmlParser().Parse(XmlSamples.SimpleRss);
    var entryElement = tree.XPathSelectElement("//item").ShouldNotBeNull();
    var entry = new Entry(entryElement, xmlNamespaceManager);

    // Act
    var titleNode = entry.TitleNode;

    // Assert
    titleNode.ShouldNotBeNull();
    titleNode.Name.NamespaceName.ShouldBeEmpty();
    titleNode.Name.LocalName.ShouldBe("title");
  }

  [TestMethod]
  public void TitleNode_AtomEntry_ReturnsNodeNamedTitle() {
    // Arrange
    var (tree, xmlNamespaceManager) = new XmlParser().Parse(XmlSamples.SimpleAtom);
    var entryElement = tree.XPathSelectElement("//atom:entry", xmlNamespaceManager).ShouldNotBeNull();
    var entry = new Entry(entryElement, xmlNamespaceManager);

    // Act
    var titleNode = entry.TitleNode;

    // Assert
    titleNode.ShouldNotBeNull();
    titleNode.Name.NamespaceName.ShouldBe("http://www.w3.org/2005/Atom");
    titleNode.Name.LocalName.ShouldBe("title");
  }

  [TestMethod]
  public void Title_RssEntry_ReturnsTitle() {
    // Arrange
    var (tree, xmlNamespaceManager) = new XmlParser().Parse(XmlSamples.SimpleRss);
    var entryElement = tree.XPathSelectElement("//item").ShouldNotBeNull();
    var entry = new Entry(entryElement, xmlNamespaceManager);

    // Act & Assert
    entry.Title.ShouldBe("Hello world");
  }

  [TestMethod]
  public void Title_AtomEntry_ReturnsTitle() {
    // Arrange
    var (tree, xmlNamespaceManager) = new XmlParser().Parse(XmlSamples.SimpleAtom);
    var entryElement = tree.XPathSelectElement("//atom:entry", xmlNamespaceManager).ShouldNotBeNull();
    var entry = new Entry(entryElement, xmlNamespaceManager);

    // Act & Assert
    entry.Title.ShouldBe("Hello world");
  }

  [TestMethod]
  public void ToString_RssEntry_ReturnsTitle() {
    // Arrange
    var (tree, xmlNamespaceManager) = new XmlParser().Parse(XmlSamples.SimpleRss);
    var entryElement = tree.XPathSelectElement("//item").ShouldNotBeNull();
    var entry = new Entry(entryElement, xmlNamespaceManager);

    // Act & Assert
    entry.ToString().ShouldBe("Hello world");
  }

  [TestMethod]
  public void ToString_AtomEntry_ReturnsTitle() {
    // Arrange
    var (tree, xmlNamespaceManager) = new XmlParser().Parse(XmlSamples.SimpleAtom);
    var entryElement = tree.XPathSelectElement("//atom:entry", xmlNamespaceManager).ShouldNotBeNull();
    var entry = new Entry(entryElement, xmlNamespaceManager);

    // Act & Assert
    entry.ToString().ShouldBe("Hello world");
  }

  [TestMethod]
  public void XPathSelectElement_RssEntry_MatchesWithNamespaceManager() {
    // Arrange
    var (tree, xmlNamespaceManager) = new XmlParser().Parse(XmlSamples.SimpleRss);
    var entryElement = tree.XPathSelectElement("//content:encoded", xmlNamespaceManager).ShouldNotBeNull();
    var entry = new Entry(entryElement, xmlNamespaceManager);

    // Act
    var nodes = entry.XPathSelectElements("//content:encoded");

    // Assert
    var node = nodes.ShouldHaveSingleItem();
    node.Value.Trim().ShouldBe("<p>Hello, world!</p>");
  }

  [TestMethod]
  public void XPathSelectElement_AtomEntry_MatchesWithNamespaceManager() {
    // Arrange
    var (tree, xmlNamespaceManager) = new XmlParser().Parse(XmlSamples.SimpleAtom);
    var entryElement = tree.XPathSelectElement("//atom:entry", xmlNamespaceManager).ShouldNotBeNull();
    var entry = new Entry(entryElement, xmlNamespaceManager);

    // Act
    var nodes = entry.XPathSelectElements("//atom:published");

    // Assert
    var node = nodes.ShouldHaveSingleItem();
    node.Value.ShouldBe("1970-01-01T00:00:00.00Z");
  }

  [TestMethod]
  public void Remove_Entry_RemovesEntry() {
    // Arrange
    var (tree, xmlNamespaceManager) = new XmlParser().Parse(XmlSamples.TwoPostRss);
    tree.XPathSelectElements("//item").Count().ShouldBe(2);
    var entryElement = tree.XPathSelectElement("//item[1]").ShouldNotBeNull();
    var entry = new Entry(entryElement, xmlNamespaceManager);
    entry.Title.ShouldBe("Post 2");

    // Act
    entry.Remove();

    // Assert
    var remainingEntryElement = tree.XPathSelectElements("//item").ShouldHaveSingleItem();
    new Entry(remainingEntryElement, xmlNamespaceManager).Title.ShouldBe("Hello world");
  }
}
