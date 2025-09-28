# FeedFilter

A proxy for RSS/Atom feeds that removes entries the user would prefer not to see.

This can help remove advertisements, posts that link to podcasts, posts from uninteresting categories, posts by specific authors.

## XML hell

RSS and Atom feeds are XML. The "X" in "XML" stands for "Extensible", and one of the ways in which this extensibility is used are XML namespaces.

While RSS and Atom are separate formats, many RSS feeds found in the wild will include `xmlns:atom="http://www.w3.org/2005/Atom"` and have some Atom tags in them (`atom:link` seems to be most common). Similarly, WordPress likes to define the post author using `dc:creator` (from `xmlns:dc="http://purl.org/dc/elements/1.1/"`) and the post content in `content:encoded` (from `xmlns:content="http://purl.org/rss/1.0/modules/content/"`).

All the built-in rule field types have several XPath expressions associated with them, based on some feeds found in the wild. However, some feeds may have other ways to express some content.

The .NET `System.Xml.Linq` library requires namespaces to be included in XPath expressions. Moreover, namespaces must be registered before they are used. The `atom`, `dc`, and `content` namespaces explained above are registered. Alternatively, `*[local-name()='example']` can be used in XPath expressions to get `<[anything]:example>`.

## Configuration

Feeds and their filtering rules must be configured. There are two ways to configure the feeds: by using the API or the UI.

The configuration API and UI must be enabled in `appsettings.json`:

```json
{
  "Admin": {
    "Token": "your-secret-token",
    "ApiEnabled": true,
    "UiEnabled": true
  }
}
```

### Configuration UI

The configuration UI can be accessed in your Web browser by going to the `/_admin` endpoint of your FeedFilter server.

### Configuration API

All requests to the configuration API must have the token specified using an `Authorization: Bearer your-secret-token` header.

Each field must have a unique ID, which becomes the URL under which the feed can be retrieved.

To configure a feed, make a POST request to `/api/feeds/{feedId}`:

```typescript
{
  "description": "feed description",
  "url": "feed URL",
  "defaultDecision": "accept" | "reject" | "promote" | "demote",
  "rules": [
    {
      "index": 0, // unique within a feed, determines rule sort order, first rule to match wins
      "field": "title" | "author" | "link" | "category" | "content" | "custom",
      "customXPath": "string" | null, // if field == "custom", the XPath of the element to test
      "testedAttributeName": "string" | null, // if null, will test the field contents
      "testType": 0,
      "testExpression": "exact" | "contains" | "startsWith" | "endsWith" | "regex", // regex is case-insensitive, remaining types are case-sensitive
      "decision": "accept" | "reject" | "promote" | "demote",
      "comment": "string" | null
    }
  ]
}
```

If the feed is saved successfully, the filtered feed will be available at `/{feedId}`. You can also POST to `/api/feeds/{feedId}/test` to see which rule was applied to which entry.

The decisions are interpreted as follows:

* `accept` keeps the post in the feed and makes no changes
* `reject` removes the post from the feed
* `promote` prepends a star emoji to the post title
* `demote` prepends a trash can emoji to the post title

#### Example feed configuration call

```json
{
  "description": "Example",
  "url": "https://example.com/feed/",
  "defaultDecision": "accept",
  "rules": [
    {
      "index": 10,
      "field": "author",
      "testType": "exact",
      "testExpression": "John Doe",
      "decision": "demote",
      "comment": "Warn about John Doe's terrible writing"
    },
    {
      "index": 20,
      "field": "title",
      "testType": "contains",
      "testExpression": "[Sponsor]",
      "decision": "reject",
      "comment": "Remove ads"
    }
  ]
}
```

#### All endpoints

* `GET /api/feeds` and `GET /api/feeds/{feedId}` to see what is configured
* `POST /api/feeds/{feedId}` to create/update a feed
* `POST /api/feeds/{feedId}/test` to test a feed (perform filtering and show information about rules applied to entries)
* `DELETE /api/feeds/{feedId}` to delete a feed
* `POST /api/feeds` to create/update many feeds
* `POST /api/test` to test a feed without configuring it

## Roadmap

The filtering appears to be working on a handful of feeds I care about, produced by various generators (including, but not limited to, WordPress). There are still some more things to work on before this becomes usable. See [GitHub Issues](https://github.com/K-Wojciechowski/FeedFilter/issues) for more details.

## Legal

This code is licensed under the MIT license.

The icon is based on [work by the Mozilla Foundation](https://commons.wikimedia.org/wiki/File:Feed-icon.svg), licensed under the MPL 1.1 license.

None of the C# code in the `src` folder was created with AI/LLM assistance. Some of the TypeScript code was refactored by Claude Sonnet.
