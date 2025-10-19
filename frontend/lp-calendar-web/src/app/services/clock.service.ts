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
    this.startAlignedClock();
  }

  private startAlignedClock() {
    const now = Date.now();
    const delay = 1000 - (now % 1000); // ms until next full second

    // wait until next full second
    setTimeout(() => {
      // fire immediately at that exact second
      this.tick();

      // then continue every second, aligned
      setInterval(() => this.tick(), 1000);
    }, delay);
  }

  private tick() {
    this.clockSubject.next(new Date());
    this.luxonClockSubject.next(DateTime.now());
  }
}
