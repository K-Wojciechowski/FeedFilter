namespace FeedFilter.Core.Test;

internal static class XmlSamples {
  public const string SimpleRss =
      """
      <?xml version="1.0" encoding="UTF-8"?>
      <rss version="2.0"
      	xmlns:content="http://purl.org/rss/1.0/modules/content/"
      	xmlns:wfw="http://wellformedweb.org/CommentAPI/"
      	xmlns:dc="http://purl.org/dc/elements/1.1/"
      	xmlns:atom="http://www.w3.org/2005/Atom"
      	xmlns:sy="http://purl.org/rss/1.0/modules/syndication/"
      	xmlns:slash="http://purl.org/rss/1.0/modules/slash/"
      	>

        <channel>
        	<title>example feed</title>
        	<link>https://example.com</link>
        	<item>
        		<title>Hello world</title>
        		<link>https://example.com/?p=1</link>
            <comments>https://example.com/?p=1#comments</comments>

        		<dc:creator><![CDATA[author]]></dc:creator>
        		<pubDate>Thu, 1 Jan 1970 00:00:00 +0000</pubDate>
        				<category><![CDATA[Uncategorized]]></category>
        		<guid isPermaLink="false">https://example.com/?p=1</guid>

            <description><![CDATA[Hello, world!]]></description>
            <content:encoded><![CDATA[
      <p>Hello, world!</p>
      ]]></content:encoded>
      			</item>
      	</channel>
      </rss>
      """;

  public const string TwoPostRss =
      """
      <?xml version="1.0" encoding="UTF-8"?>
      <rss version="2.0"
      	xmlns:content="http://purl.org/rss/1.0/modules/content/"
      	xmlns:wfw="http://wellformedweb.org/CommentAPI/"
      	xmlns:dc="http://purl.org/dc/elements/1.1/"
      	xmlns:atom="http://www.w3.org/2005/Atom"
      	xmlns:sy="http://purl.org/rss/1.0/modules/syndication/"
      	xmlns:slash="http://purl.org/rss/1.0/modules/slash/"
      	>

        <channel>
        	<title>example feed</title>
        	<link>https://example.com</link>
        	<item>
        		<title>Post 2</title>
        		<link>https://example.com/?p=2</link>
            <comments>https://example.com/?p=2#comments</comments>

        		<dc:creator><![CDATA[author]]></dc:creator>
        		<pubDate>Thu, 1 Jan 1970 00:00:00 +0000</pubDate>
        				<category><![CDATA[Uncategorized]]></category>
        		<guid isPermaLink="false">https://example.com/?p=2</guid>

            <description><![CDATA[Post 2!]]></description>
            <content:encoded><![CDATA[
      <p>Post 2!</p>
      ]]></content:encoded>
      			</item>

        	<item>
        		<title>Hello world</title>
        		<link>https://example.com/?p=1</link>
            <comments>https://example.com/?p=1#comments</comments>

        		<dc:creator><![CDATA[author]]></dc:creator>
        		<pubDate>Thu, 1 Jan 1970 00:00:00 +0000</pubDate>
        				<category><![CDATA[Uncategorized]]></category>
        		<guid isPermaLink="false">https://example.com/?p=1</guid>

            <description><![CDATA[Hello, world!]]></description>
            <content:encoded><![CDATA[
      <p>Hello, world!</p>
      ]]></content:encoded>
      			</item>
      	</channel>
      </rss>
      """;

  public const string SimpleAtom =
      """
      <?xml version="1.0" encoding="UTF-8"?>
      <feed xmlns="http://www.w3.org/2005/Atom" xml:lang="en">
        <title>example feed</title>
        <link rel="alternate" type="text/html" href="https://example.com/"/>
        <entry>
          <id>00000</id>
          <published>1970-01-01T00:00:00.00Z</published>
          <updated>1970-01-01T00:00:00.00Z</updated>
          <author>
            <name>John Doe</name>
            <email>john.doe@example.com</email>
          </author>
          <link rel="alternate" type="text/html" href="https://example.com/?p=1"/>
          <title type="html">Hello world</title>
          <summary type="html" xml:base="https://www.theregister.com/">&lt;p&gt;Hello, world!&lt;/p&gt;</summary>
        </entry>
      </feed>
      """;
}
