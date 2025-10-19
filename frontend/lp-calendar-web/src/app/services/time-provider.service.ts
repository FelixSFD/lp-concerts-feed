import { Injectable } from '@angular/core';
import {BehaviorSubject} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TimeProviderService {
  private clockSubject = new BehaviorSubject<number>(Date.now());
  clock$ = this.clockSubject.asObservable();

  constructor() {
    setInterval(() => { this.clockSubject.next(Date.now()); }, 1000);
  }
}
