import { environment } from '../../../../environments/environment';

/** Returns the WebCal iCal feed URL for a subscription. */
export function calendarSubscriptionFeedUrl(subscriptionId: string): string {
  const baseUrl = new URL(environment.calendarSubscriptionFeedBaseUrl);
  if (baseUrl.protocol !== 'https:' || baseUrl.search || baseUrl.hash) {
    throw new Error('The calendar subscription feed base URL must be an HTTPS URL without a query or fragment.');
  }

  baseUrl.pathname = `${baseUrl.pathname.replace(/\/+$/, '')}/${encodeURIComponent(subscriptionId)}/subscription.ical`;
  return baseUrl.href.replace(/^https:/, 'webcal:');
}
