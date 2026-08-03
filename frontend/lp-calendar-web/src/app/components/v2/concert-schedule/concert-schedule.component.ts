import { Component, inject, Input } from '@angular/core';
import { Tooltip } from 'primeng/tooltip';
import { DateTime } from 'luxon';
import { ConcertDto } from '../../../modules/lpshows-api';
import { DiscordShareService } from '../../../services/discord-share.service';

export interface ScheduleStep {
  key: string;
  label: string;
  isoTime: string | null;
  venueTime: string;
  readerTime: string;
  note: string;
  badge: string;
  badgeKind: 'ok' | 'tbc' | 'none';
  isLead: boolean;
}

@Component({
  selector: 'app-concert-schedule',
  imports: [Tooltip],
  templateUrl: './concert-schedule.component.html',
  styleUrl: './concert-schedule.component.css',
})
export class ConcertScheduleComponent {
  @Input() concert: ConcertDto | null = null;

  @Input() showDiscordShare: boolean = false;

  @Input() heading: string = 'Schedule';

  protected readonly discordShare = inject(DiscordShareService);

  showVenueTime: boolean = true;

  setTimeMode(venueTime: boolean): void {
    this.showVenueTime = venueTime;
  }

  get isPast(): boolean {
    return this.concert?.isPast === true;
  }

  get timezonesDiffer(): boolean {
    const concert = this.concert;
    const reference =
      concert?.doorsTime ?? concert?.mainStageTime ?? concert?.postedStartTime;
    if (!concert?.timeZoneId || !reference) {
      return false;
    }

    return (
      DateTime.fromISO(reference, { zone: concert.timeZoneId }).offset !==
      DateTime.fromISO(reference).offset
    );
  }

  get steps(): ScheduleStep[] {
    const concert = this.concert;
    if (concert == null) {
      return [];
    }

    const steps: ScheduleStep[] = [
      {
        key: 'lpu',
        label: 'LPU early entry',
        isoTime: concert.lpuEarlyEntryTime ?? null,
        venueTime: this.atVenue(concert.lpuEarlyEntryTime),
        readerTime: this.forReader(concert.lpuEarlyEntryTime),
        note: '',
        ...this.lpuBadge(concert),
        isLead: false,
      },
      {
        key: 'doors',
        label: 'Doors open',
        isoTime: concert.doorsTime ?? null,
        venueTime: this.atVenue(concert.doorsTime),
        readerTime: this.forReader(concert.doorsTime),
        note: '',
        badge: concert.doorsTime ? 'Confirmed' : 'Not published',
        badgeKind: concert.doorsTime ? 'ok' : 'none',
        isLead: false,
      },
    ];

    if (
      concert.postedStartTime &&
      concert.postedStartTime !== concert.doorsTime
    ) {
      steps.push({
        key: 'start',
        label: 'Show starts',
        isoTime: concert.postedStartTime,
        venueTime: this.atVenue(concert.postedStartTime),
        readerTime: this.forReader(concert.postedStartTime),
        note: 'On the ticket',
        badge: 'Confirmed',
        badgeKind: 'ok',
        isLead: false,
      });
    }

    steps.push({
      key: 'stage',
      label: 'LP stage time',
      isoTime: concert.mainStageTime ?? null,
      venueTime: this.atVenue(concert.mainStageTime),
      readerTime: this.forReader(concert.mainStageTime),
      note: '',
      badge: concert.mainStageTime ? 'Confirmed' : 'Not published',
      badgeKind: concert.mainStageTime ? 'ok' : 'none',
      isLead: true,
    });

    return steps;
  }

  private lpuBadge(concert: ConcertDto): {
    badge: string;
    badgeKind: 'ok' | 'tbc' | 'none';
  } {
    if (!concert.lpuEarlyEntryTime) {
      return { badge: 'Not published', badgeKind: 'none' };
    }

    return concert.lpuEarlyEntryConfirmed
      ? { badge: 'Confirmed', badgeKind: 'ok' }
      : { badge: 'Not confirmed', badgeKind: 'tbc' };
  }

  /** "2 hours" / "105 min" — only when the set length is known. */
  get setDurationLabel(): string | null {
    const minutes = this.concert?.expectedSetDuration;
    if (!minutes) {
      return null;
    }

    if (minutes % 60 === 0) {
      const hours = minutes / 60;
      return hours === 1 ? '1 hour' : `${hours} hours`;
    }

    return `${minutes} min`;
  }

  private atVenue(isoTime: string | null | undefined): string {
    if (!isoTime) {
      return '';
    }

    const zone = this.concert?.timeZoneId;
    const dateTime = zone
      ? DateTime.fromISO(isoTime, { zone })
      : DateTime.fromISO(isoTime);
    return dateTime.toLocaleString(DateTime.TIME_SIMPLE);
  }

  private forReader(isoTime: string | null | undefined): string {
    return isoTime
      ? DateTime.fromISO(isoTime).toLocaleString(DateTime.TIME_SIMPLE)
      : '';
  }
}
