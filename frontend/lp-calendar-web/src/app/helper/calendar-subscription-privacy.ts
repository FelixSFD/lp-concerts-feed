/**
 * Removes the management capability and query parameters from subscription detail URLs before tracking.
 * @param url Absolute or relative page URL.
 * @returns An absolute URL with subscription detail paths redacted.
 */
export function sanitizeCalendarSubscriptionUrl(url: string): string {
  const parsed = new URL(url, window.location.origin);
  if (/^\/calendar-subscriptions\/[^/]+\/?$/i.test(parsed.pathname)) {
    parsed.pathname = '/calendar-subscriptions/manage';
    parsed.search = '';
    parsed.hash = '';
  }
  return parsed.href;
}
