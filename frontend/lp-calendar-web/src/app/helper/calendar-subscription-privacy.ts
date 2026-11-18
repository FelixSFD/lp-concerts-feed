export function sanitizeCalendarSubscriptionUrl(url: string): string {
  const parsed = new URL(url, window.location.origin);
  if (/^\/calendar-subscriptions\/[^/]+\/?$/i.test(parsed.pathname)) {
    parsed.pathname = '/calendar-subscriptions/manage';
    parsed.search = '';
    parsed.hash = '';
  }
  return parsed.href;
}
