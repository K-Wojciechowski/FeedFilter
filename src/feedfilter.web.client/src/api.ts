import type { Feed, FeedFilteringResult, FeedUpdate } from "./models.ts";

const fetchOptions = (token: string): RequestInit => ({
  headers: { Authorization: "Bearer " + token },
  signal: AbortSignal.timeout(7500),
});

const postOptions = (token: string): RequestInit => ({
  headers: { Authorization: "Bearer " + token, "Content-Type": "application/json" },
  signal: AbortSignal.timeout(7500),
});

export const getFeeds = async (token: string): Promise<Feed[]> => {
  const response = await fetch("/api/feeds/", fetchOptions(token));
  if (response.ok) {
    return await response.json();
  } else {
    throw new Error("Invalid token");
  }
};

export const saveFeed = async (
  token: string,
  feedId: string,
  feedUpdate: FeedUpdate,
): Promise<Feed> => {
  const response = await fetch("/api/feeds/" + feedId, {
    ...postOptions(token),
    method: "post",
    body: JSON.stringify(feedUpdate),
  });
  if (response.ok) {
    return await response.json();
  } else {
    throw new Error(await response.text());
  }
};

export const deleteFeed = async (token: string, feedId: string): Promise<void> => {
  const response = await fetch("/api/feeds/" + feedId, {
    ...fetchOptions(token),
    method: "delete",
  });
  if (!response.ok) {
    throw new Error(await response.text());
  }
};

export const testFeed = async (
  token: string,
  feedUpdate: FeedUpdate,
): Promise<FeedFilteringResult> => {
  const response = await fetch("/api/test", {
    ...postOptions(token),
    method: "post",
    body: JSON.stringify(feedUpdate),
  });
  if (response.ok) {
    return await response.json();
  } else {
    throw new Error(await response.text());
  }
};
