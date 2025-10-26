export function getLink(feedId: string): string {
  const urlString: string = location.href;
  const url = new URL(urlString);
  url.search = "";
  url.hash = "";
  url.pathname += url.pathname.endsWith("/") ? feedId : `/${feedId}`;
  return url.toString();
}

export async function copyLink(feedId: string) {
  await navigator.clipboard.writeText(getLink(feedId));
}
