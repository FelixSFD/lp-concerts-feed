import { Injectable } from '@angular/core';
import {BehaviorSubject} from 'rxjs';
import {DateTime} from 'luxon';

/**
 * This service publishes the current time every second.
 * By using this, we can make sure that all countdowns on the same page will update at the same millisecond
 */
@Injectable({
  providedIn: 'root'
})
export class ClockService {
  private clockSubject = new BehaviorSubject<Date>(new Date());
  private luxonClockSubject = new BehaviorSubject<DateTime>(DateTime.now());
  clock$ = this.clockSubject.asObservable();
  luxonClock$ = this.luxonClockSubject.asObservable();

  constructor() {
    setInterval(() => {
      this.clockSubject.next(new Date());
      this.luxonClockSubject.next(DateTime.now());
      }, 1000);
  }
}
