import { AfterViewInit, ChangeDetectionStrategy, Component, inject, Input, OnInit } from '@angular/core';
import { ClockService } from '../../../services/clock.service';
import { DecimalPipe } from '@angular/common';
import { DiscordShareService } from '../../../services/discord-share.service';
import { Popover } from 'primeng/popover';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { InputText } from 'primeng/inputtext';
import { Button } from 'primeng/button';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-hero-countdown',
  imports: [
    DecimalPipe,
    Popover,
    InputGroup,
    InputGroupAddon,
    InputText,
    Button
  ],
  templateUrl: './hero-countdown.component.html',
  styleUrl: './hero-countdown.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})

export class HeroCountdownComponent implements OnInit, AfterViewInit {
  private clockService = inject(ClockService);
  protected readonly discordShare = inject(DiscordShareService);

  differenceMillis$ = 0;
  days$ = 0;
  hours$ = 0;
  minutes$ = 0;
  seconds$ = 0;

  discordFull = '';
  discordRelative = '';

  @Input()
  countdownToDate!: string | DateTime;

  ngOnInit() {
    this.updateView();
    this.discordFull = this.discordShare.timestamp(this.countdownToDate, 'F');
    this.discordRelative = this.discordShare.timestamp(this.countdownToDate, 'R');
  }

  ngAfterViewInit() {
    this.clockService.clock$.subscribe(() => this.updateView());
  }

  private updateView(): void {
    const now = new Date();
    const target = this.countdownToDate instanceof DateTime
      ? this.countdownToDate.toJSDate()
      : new Date(this.countdownToDate);
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
