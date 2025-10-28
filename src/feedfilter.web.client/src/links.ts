export function getLink(feedId: string): string {
  const url = new URL(location.href);
  url.search = "";
  url.hash = "";
  url.pathname = url.pathname.replace(/\/_admin(\/.*)?$/, "/") + feedId;
  return url.toString();
}

export async function copyLink(feedId: string) {
  await navigator.clipboard.writeText(getLink(feedId));
}
