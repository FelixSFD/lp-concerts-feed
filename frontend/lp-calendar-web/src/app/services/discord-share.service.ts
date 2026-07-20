import {inject, Injectable} from '@angular/core';
import {DateTime} from 'luxon';
import {MessageService} from 'primeng/api';
import {MatomoTracker} from 'ngx-matomo-client';

export type DiscordTimestampStyle = 't' | 'T' | 'd' | 'D' | 'f' | 'F' | 'R';

/**
 * Builds and copies Discord timestamps (e.g. <t:1785000000:F>)
 **/
@Injectable({providedIn: 'root'})
export class DiscordShareService {
  private readonly messageService = inject(MessageService);
  private readonly tracker = inject(MatomoTracker);

  timestamp(inputDate: string, style: DiscordTimestampStyle = 'F'): string {
    const seconds = Math.floor(DateTime.fromISO(inputDate).toSeconds());
    return `<t:${seconds}:${style}>`;
  }

  copyTimestamp(inputDate: string | undefined | null, style: DiscordTimestampStyle = 'F', label = "timestamp"): void {
    if (!inputDate) {
      return;
    }
    navigator.clipboard.writeText(this.timestamp(inputDate, style)).then(() => {
      this.messageService.add({severity: "success", summary: "Copied for Discord!"});
      this.tracker.trackEvent("share_discord", label, style);
    });
  }
}
