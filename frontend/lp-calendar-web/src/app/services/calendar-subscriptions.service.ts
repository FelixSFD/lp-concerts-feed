import { inject, Injectable } from '@angular/core';
import { CalendarSubscriptionDto, CalendarSubscriptionSettingsDto, CalendarSubscriptionsApi } from '../modules/lpshows-api/v3';
import { environment } from '../../environments/environment';
import { map, Observable } from 'rxjs';
import { addAuthentication } from '../auth/auth.config';

@Injectable({ providedIn: 'root' })
export class CalendarSubscriptionsService {
  private readonly api = inject(CalendarSubscriptionsApi);

  constructor() {
    this.api.configuration.basePath = environment.apiBaseUrl;
    addAuthentication(this.api);
  }

  create(settings: CalendarSubscriptionSettingsDto): Observable<CalendarSubscriptionDto> {
    return this.api.createCalendarSubscription(settings);
  }

  listMine(): Observable<CalendarSubscriptionDto[]> {
    return this.api.getMyCalendarSubscriptions();
  }

  get(id: string): Observable<CalendarSubscriptionDto> {
    return this.api.getCalendarSubscription(id);
  }

  update(id: string, settings: CalendarSubscriptionSettingsDto): Observable<void> {
    return this.api.updateCalendarSubscription(id, settings).pipe(map(() => undefined));
  }

  delete(id: string): Observable<void> {
    return this.api.deleteCalendarSubscription(id).pipe(map(() => undefined));
  }

  claim(id: string): Observable<CalendarSubscriptionDto> {
    return this.api.claimCalendarSubscription(id);
  }
}
