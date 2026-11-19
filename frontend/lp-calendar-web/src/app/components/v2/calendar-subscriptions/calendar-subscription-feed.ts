import { environment } from '../../../../environments/environment';

/** Opens a subscription's iCal feed in the user's calendar application. */
export function openCalendarSubscriptionFeed(subscriptionId: string): void {
  const baseUrl = new URL(environment.calendarSubscriptionFeedBaseUrl);
  if (baseUrl.protocol !== 'https:' || baseUrl.search || baseUrl.hash) {
    throw new Error('The calendar subscription feed base URL must be an HTTPS URL without a query or fragment.');
  }

  baseUrl.pathname = `${baseUrl.pathname.replace(/\/+$/, '')}/${encodeURIComponent(subscriptionId)}/subscription.ical`;
  window.open(baseUrl.href.replace(/^https:/, 'webcal:'));
}
