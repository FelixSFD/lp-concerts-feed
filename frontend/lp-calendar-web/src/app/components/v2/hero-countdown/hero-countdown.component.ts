import {AfterViewInit, Component, inject, Input, OnInit} from '@angular/core';
import {ClockService} from '../../../services/clock.service';
import {DecimalPipe} from '@angular/common';
import {DiscordShareService} from '../../../services/discord-share.service';

@Component({
  selector: 'app-hero-countdown',
  imports: [
    DecimalPipe
  ],
  templateUrl: './hero-countdown.component.html',
  styleUrl: './hero-countdown.component.css'
})
export class HeroCountdownComponent implements OnInit, AfterViewInit {
  private clockService = inject(ClockService);
  protected readonly discordShare = inject(DiscordShareService);

  differenceMillis$ = 0;
  days$ = 0;
  hours$ = 0;
  minutes$ = 0;
  seconds$ = 0;

  @Input()
  countdownToDate!: string;

  ngOnInit() {
    this.updateView();
  }

  ngAfterViewInit() {
    this.clockService.clock$.subscribe(() => this.updateView());
  }

  private updateView(): void {
    const now = new Date();
    const target = new Date(this.countdownToDate);
    const difference = target.getTime() - now.getTime();
    this.differenceMillis$ = difference;

    if (difference < 0) {
      return;
    }

    this.days$ = Math.floor(difference / (24 * 60 * 60 * 1000));
    this.hours$ = Math.floor((difference % (24 * 60 * 60 * 1000)) / (60 * 60 * 1000));
    this.minutes$ = Math.floor((difference % (60 * 60 * 1000)) / (60 * 1000));
    this.seconds$ = Math.floor((difference % (60 * 1000)) / 1000);
  }
}
