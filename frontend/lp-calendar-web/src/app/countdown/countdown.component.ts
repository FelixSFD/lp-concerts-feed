import {AfterViewInit, Component, ElementRef, Input, ViewChild} from '@angular/core';
import {NgIf} from '@angular/common';

@Component({
  selector: 'app-countdown',
  imports: [
    NgIf
  ],
  templateUrl: './countdown.component.html',
  styleUrl: './countdown.component.css'
})
export class CountdownComponent implements AfterViewInit {
  differenceMillis$: number = 0;
  days$: number = 0;
  hours$: number = 0;
  minutes$: number = 0;
  seconds$: number = 0;

  @Input()
  countdownToDate!: string;

  ngAfterViewInit() {
    setInterval(() => {
      let now = new Date();
      let target = new Date(this.countdownToDate);

      let difference = target.getTime() - now.getTime();
      this.differenceMillis$ = difference;
      if (difference < 0) {
        return;
      }

      this.days$ = Math.floor(difference / (24 * 60 * 60 * 1000));
      let daysFromMilliSeconds = difference % (24 * 60 * 60 * 1000);
      this.hours$ = Math.floor((daysFromMilliSeconds) / (60 * 60 * 1000));
      let hoursFromMilliSeconds = difference % (60 * 60 * 1000);
      this.minutes$ = Math.floor((hoursFromMilliSeconds) / (60 * 1000));
      let minutesFromMilliSeconds = difference % (60 * 1000);
      this.seconds$ = Math.floor((minutesFromMilliSeconds) / (1000));
    }, 1000);
  }
}
