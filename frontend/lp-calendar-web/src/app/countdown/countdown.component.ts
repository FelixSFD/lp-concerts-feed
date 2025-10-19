import {AfterViewInit, Component, ElementRef, inject, Input, OnInit, ViewChild} from '@angular/core';
import {NgIf} from '@angular/common';
import {environment} from '../../environments/environment';
import {ToastrService} from 'ngx-toastr';
import {MatomoTrackClickDirective} from 'ngx-matomo-client';
import {ClockService} from '../services/clock.service';

@Component({
  selector: 'app-countdown',
  imports: [
    NgIf,
    MatomoTrackClickDirective
  ],
  templateUrl: './countdown.component.html',
  styleUrl: './countdown.component.css'
})
export class CountdownComponent implements OnInit, AfterViewInit {
  private toastrService = inject(ToastrService);
  private clockService = inject(ClockService);

  differenceMillis$: number = 0;
  days$: number = 0;
  hours$: number = 0;
  minutes$: number = 0;
  seconds$: number = 0;

  @Input()
  countdownToDate!: string;

  @Input()
  concertId: string | undefined;

  ngOnInit() {
    // one initial update of the countdown values
    this.updateView();
  }

  ngAfterViewInit() {
    // then trigger an update every time the global clock updates
    this.clockService.clock$.subscribe(clock => {
      //console.log("Will update countdown for: ", this.concertId, clock);
      this.updateView();
    });
  }


  onShareDiscordTsClicked(type: string) {
    let target = new Date(this.countdownToDate);
    let utc_timestamp = Date.UTC(target.getUTCFullYear(),target.getUTCMonth(), target.getUTCDate() ,
      target.getUTCHours(), target.getUTCMinutes(), target.getUTCSeconds(), 0) / 1000; // divide by 1000 because discord wants seconds

    let discordTimeStamp = `<t:${utc_timestamp}:${type}>`;
    console.debug("Generated timestamp: " + discordTimeStamp);
    navigator.clipboard.writeText(discordTimeStamp)
      .then(_ => {
        this.toastrService.success("Copied to clipboard!");
      });
  }


  onShareLinkClicked() {
    let link = window.location.protocol + "//" + window.location.host + "/concerts/" + this.concertId;
    navigator.clipboard.writeText(link)
      .then(_ => {
        this.toastrService.success("Copied link to clipboard!");
      });
  }


  private updateView(): void {
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
  }
}
