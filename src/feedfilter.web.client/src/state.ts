import { create } from "zustand";
import { devtools } from "zustand/middleware";
import type {} from "@redux-devtools/extension"; // required for devtools typing
import type { Feed, FeedUpdate } from "./models.ts";
import { deleteFeed, getFeeds, saveFeed } from "./api.ts";

interface FeedFilterState {
  feeds: Feed[];
  loading: boolean;
  token: string | undefined;
  init: (token: string) => Promise<void>;
  save: (feedId: string, feedUpdate: FeedUpdate) => Promise<void>;
  delete: (feedId: string) => Promise<void>;
}

const sortFeeds = (feeds: Feed[]): Feed[] =>
  feeds.sort((a, b) => {
    if (a < b) {
      return -1;
    }
    if (a > b) {
      return 1;
    }
    return 0;
  });

const useFeedFilterStore = create<FeedFilterState>()(
  devtools((set, get) => ({
    feeds: [],
    loading: false,
    token: undefined,
    init: async (token: string) => {
      set({ loading: true });
      try {
        const feeds = await getFeeds(token);
        set({
          token,
          loading: false,
          feeds: sortFeeds(feeds),
        });
      } finally {
        set({ loading: false });
      }
    },
    save: async (feedId: string, feedUpdate: FeedUpdate) => {
      const token = get().token!;

      set({ loading: true });
      try {
        const newFeed = await saveFeed(token, feedId, feedUpdate);
        const currentFeeds = get().feeds;
        set({
          feeds: sortFeeds([...currentFeeds.filter((feed) => feed.feedId !== feedId), newFeed]),
        });
      } finally {
        set({ loading: false });
      }
    },
    delete: async (feedId: string) => {
      const token = get().token!;

      set({ loading: true });
      try {
        await deleteFeed(token, feedId);
        const currentFeeds = get().feeds;
        set({ feeds: currentFeeds.filter((feed) => feed.feedId !== feedId) });
      } finally {
        set({ loading: false });
      }
    },
  })),
);

export default useFeedFilterStore;
