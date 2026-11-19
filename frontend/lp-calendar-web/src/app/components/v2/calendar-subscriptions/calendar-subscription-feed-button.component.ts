import { Component, inject, input, signal } from '@angular/core';
import { MenuItem, MessageService } from 'primeng/api';
import { SplitButton } from 'primeng/splitbutton';
import { calendarSubscriptionFeedUrl } from './calendar-subscription-feed';

/** Provides actions to open or copy a subscription's iCal feed. */
@Component({
  selector: 'app-calendar-subscription-feed-button',
  imports: [SplitButton],
  template: `
    <p-splitbutton label="Install feed" icon="pi pi-calendar-plus" [size]="size()" [outlined]="false"
      expandAriaLabel="Feed options" [model]="items()" appendTo="body" (onClick)="openFeed()" />
  `,
})
export class CalendarSubscriptionFeedButtonComponent {
  readonly subscriptionId = input.required<string>();
  readonly size = input<'small' | undefined>();
  private readonly messages = inject(MessageService);

  protected readonly items = signal<MenuItem[]>([
    { label: 'Copy link', icon: 'pi pi-copy', command: () => void this.copyFeedLink() },
  ]);

  protected openFeed(): void {
    window.open(calendarSubscriptionFeedUrl(this.subscriptionId()));
  }

  private async copyFeedLink(): Promise<void> {
    const url = calendarSubscriptionFeedUrl(this.subscriptionId());
    try {
      await navigator.clipboard.writeText(url);
      this.messages.add({ severity: 'success', summary: 'Feed link copied' });
    } catch {
      this.messages.add({ severity: 'error', summary: 'Could not copy feed link', detail: 'Check your browser clipboard permissions and try again.' });
    }
  }
}
