import { DatePipe } from '@angular/common';
import { Component, computed, DestroyRef, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { ToggleSwitch } from 'primeng/toggleswitch';
import { AuthService } from '../../../auth/auth.service';
import { CalendarSubscriptionDto } from '../../../modules/lpshows-api/v3';
import { CalendarSubscriptionNavigationService } from '../../../services/calendar-subscription-navigation.service';
import { CalendarSubscriptionsService } from '../../../services/calendar-subscriptions.service';
import { subscriptionErrorMessage } from './calendar-subscription-errors';

@Component({
  selector: 'app-calendar-subscription-page',
  imports: [Button, Card, ConfirmDialog, DatePipe, ReactiveFormsModule, RouterLink, ToggleSwitch],
  templateUrl: './calendar-subscription-page.component.html',
})
export class CalendarSubscriptionPageComponent {
  private readonly auth = inject(AuthService);
  private readonly oidc = inject(OidcSecurityService);
  private readonly service = inject(CalendarSubscriptionsService);
  private readonly navigation = inject(CalendarSubscriptionNavigationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly messages = inject(MessageService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly ready = computed(() => this.auth.initialized() && !this.auth.initializationError());
  protected readonly authError = this.auth.initializationError;
  protected readonly authenticated = this.oidc.authenticated;
  protected readonly subscription = signal<CalendarSubscriptionDto | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly busy = signal(false);
  protected readonly reloadVersion = signal(0);
  protected readonly form = signal(this.formBuilder.nonNullable.group({
    includeDoorsTime: false,
    includeLpStageTime: true,
  }));
  protected readonly managementLink = signal('');
  private readonly routeParams = toSignal(this.route.paramMap, { initialValue: this.route.snapshot.paramMap });

  private readonly loadSubscription = effect(onCleanup => {
    const ready = this.ready();
    this.authenticated();
    this.oidc.userData();
    this.reloadVersion();
    const id = this.routeParams().get('subscriptionId');
    this.subscription.set(null);
    this.error.set('');
    this.loading.set(ready);
    if (!ready) return;
    if (!id || !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id)) {
      this.error.set('This subscription was not found or is not accessible to your account.');
      this.loading.set(false);
      return;
    }
    this.managementLink.set(new URL(this.router.serializeUrl(this.router.createUrlTree(['/calendar-subscriptions', id])), document.baseURI).href);
    const request = this.service.get(id).subscribe({
      next: subscription => {
        this.subscription.set(subscription);
        this.form().setValue({
          includeDoorsTime: subscription.includeDoorsTime,
          includeLpStageTime: subscription.includeLpStageTime,
        });
        this.loading.set(false);
      },
      error: error => {
        this.error.set(subscriptionErrorMessage(error));
        this.loading.set(false);
      },
    });
    onCleanup(() => request.unsubscribe());
  });

  protected save(): void {
    const subscription = this.subscription();
    if (!subscription || this.busy()) return;
    this.busy.set(true);
    this.service.update(subscription.id, this.form().getRawValue()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.busy.set(false);
        this.messages.add({ severity: 'success', summary: 'Subscription updated' });
        this.reload();
      },
      error: error => this.fail('Could not update subscription', error),
    });
  }

  protected delete(event: MouseEvent): void {
    const subscription = this.subscription();
    if (!subscription || this.busy()) return;
    this.confirmation.confirm({
      target: event.target as EventTarget,
      header: 'Delete subscription',
      message: 'Delete this subscription? This cannot be undone.',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Delete', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary' },
      accept: () => {
        if (this.busy()) return;
        this.busy.set(true);
        this.service.delete(subscription.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
          next: () => {
            this.busy.set(false);
            this.messages.add({ severity: 'success', summary: 'Subscription deleted' });
            void this.router.navigate(['/calendar-subscriptions']);
          },
          error: error => this.fail('Could not delete subscription', error),
        });
      },
    });
  }

  protected claim(event: MouseEvent): void {
    const subscription = this.subscription();
    if (!subscription || subscription.userId || this.busy() || !this.authenticated().isAuthenticated) return;
    this.confirmation.confirm({
      target: event.target as EventTarget,
      header: 'Claim subscription',
      message: 'Associate this subscription with your account? After claiming it, only your account can manage it.',
      acceptButtonProps: { label: 'Claim' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary' },
      accept: () => {
        if (this.busy() || !this.authenticated().isAuthenticated) return;
        this.busy.set(true);
        this.service.claim(subscription.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
          next: () => {
            this.busy.set(false);
            this.messages.add({ severity: 'success', summary: 'Subscription claimed' });
            this.reload();
          },
          error: error => this.fail('Could not claim subscription', error),
        });
      },
    });
  }

  protected async copyLink(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.managementLink());
      this.messages.add({ severity: 'success', summary: 'Management link copied' });
    } catch {
      this.messages.add({ severity: 'error', summary: 'Could not copy link', detail: 'Select and copy the link manually.' });
    }
  }

  protected login(): void {
    this.navigation.login();
  }

  protected reload(): void {
    this.reloadVersion.update(value => value + 1);
  }

  private fail(summary: string, error: unknown): void {
    this.busy.set(false);
    this.messages.add({ severity: 'error', summary, detail: subscriptionErrorMessage(error) });
  }
}
