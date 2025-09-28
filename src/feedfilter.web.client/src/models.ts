export type Decision = "accept" | "reject" | "promote" | "demote";
export type ItemField = "title" | "author" | "link" | "category" | "content" | "custom";
export type TestType = "exact" | "contains" | "startsWith" | "endsWith" | "regex";

export interface FeedUpdate {
  description: string;
  url: string;
  defaultDecision: Decision;
  rules: readonly Rule[];
}

export interface Feed extends FeedUpdate {
  feedId: string;
  dateCreated: string;
  dateUpdated: string;
}

export interface Rule {
  index: number;
  field: ItemField;
  customXPath: string | undefined;
  testedAttributeName: string | undefined;
  testType: TestType;
  testExpression: string;
  decision: Decision;
  comment: string | undefined;
}

export interface FeedFilteringResult {
  feed: FeedUpdate;
  originalXml: string;
  filteredXml: string;
  entryResults: EntryFilteringResult[];
}

export interface EntryFilteringResult {
  entryTitle: string | undefined;
  decidingRule: Rule | undefined;
  decision: Decision;
}
