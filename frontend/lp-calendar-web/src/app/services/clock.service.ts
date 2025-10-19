import { Injectable } from '@angular/core';
import {BehaviorSubject} from 'rxjs';

/**
 * This service publishes the current time every second.
 * By using this, we can make sure that all countdowns on the same page will update at the same millisecond
 */
@Injectable({
  providedIn: 'root'
})
export class ClockService {
  private clockSubject = new BehaviorSubject<Date>(new Date());
  clock$ = this.clockSubject.asObservable();

  constructor() {
    setInterval(() => { this.clockSubject.next(new Date()); }, 1000);
  }
}
