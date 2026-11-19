import { inject, Injectable } from '@angular/core';
import { CalendarSubscriptionDto, CalendarSubscriptionSettingsDto, CalendarSubscriptionsApi } from '../modules/lpshows-api/v3';
import { environment } from '../../environments/environment';
import { map, Observable } from 'rxjs';
import { addAuthentication } from '../auth/auth.config';

/** Provides access to calendar subscription management through the v3 API. */
@Injectable({ providedIn: 'root' })
export class CalendarSubscriptionsService {
  private readonly api = inject(CalendarSubscriptionsApi);
  constructor() {
    this.api.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(this.api);
  }

  /**
   * Creates a subscription owned by the authenticated user, or an unowned subscription when anonymous.
   * @param settings Initial event-time settings.
   */
  create(settings: CalendarSubscriptionSettingsDto): Observable<CalendarSubscriptionDto> {
    return this.api.createCalendarSubscription(settings);
  }

  /** Returns the authenticated user's subscriptions. */
  listMine(): Observable<CalendarSubscriptionDto[]> {
    return this.api.getMyCalendarSubscriptions();
  }

  /**
   * Retrieves an accessible subscription.
   * @param id Subscription ID; for unowned subscriptions, this is a secret management capability.
   */
  get(id: string): Observable<CalendarSubscriptionDto> {
    return this.api.getCalendarSubscription(id);
  }

  /**
   * Replaces the settings of an accessible subscription.
   * @param id Subscription ID.
   * @param settings Replacement event-time settings.
   */
  update(id: string, settings: CalendarSubscriptionSettingsDto): Observable<void> {
    return this.api.updateCalendarSubscription(id, settings).pipe(map(() => undefined));
  }

  /**
   * Deletes an accessible subscription.
   * @param id Subscription ID.
   */
  delete(id: string): Observable<void> {
    return this.api.deleteCalendarSubscription(id).pipe(map(() => undefined));
  }

  /**
   * Associates an unowned subscription with the authenticated user.
   * @param id Subscription ID.
   */
  claim(id: string): Observable<CalendarSubscriptionDto> {
    return this.api.claimCalendarSubscription(id);
  }
}
