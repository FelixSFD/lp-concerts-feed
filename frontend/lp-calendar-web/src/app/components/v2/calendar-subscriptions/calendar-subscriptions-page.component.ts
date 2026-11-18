import { DatePipe } from '@angular/common';
import { Component, computed, DestroyRef, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { MessageService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ToggleSwitch } from 'primeng/toggleswitch';
import { AuthService } from '../../../auth/auth.service';
import { CalendarSubscriptionDto } from '../../../modules/lpshows-api/v3';
import { CalendarSubscriptionNavigationService } from '../../../services/calendar-subscription-navigation.service';
import { CalendarSubscriptionsService } from '../../../services/calendar-subscriptions.service';
import { subscriptionErrorMessage } from './calendar-subscription-errors';

/** Creates calendar subscriptions and displays the authenticated user's subscriptions. */
@Component({
  selector: 'app-calendar-subscriptions-page',
  imports: [Button, Card, DatePipe, ReactiveFormsModule, RouterLink, TableModule, ToggleSwitch],
  templateUrl: './calendar-subscriptions-page.component.html',
})
export class CalendarSubscriptionsPageComponent {
  private readonly auth = inject(AuthService);
  private readonly oidc = inject(OidcSecurityService);
  private readonly subscriptionsService = inject(CalendarSubscriptionsService);
  private readonly navigation = inject(CalendarSubscriptionNavigationService);
  private readonly router = inject(Router);
  private readonly messages = inject(MessageService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly ready = computed(() => this.auth.initialized() && !this.auth.initializationError());
  protected readonly authError = this.auth.initializationError;
  protected readonly authenticated = this.oidc.authenticated;
  protected readonly form = signal(this.formBuilder.nonNullable.group({
    includeDoorsTime: false,
    includeLpStageTime: true,
  }));
  protected readonly subscriptions = signal<CalendarSubscriptionDto[]>([]);
  protected readonly loading = signal(false);
  protected readonly listError = signal('');
  protected readonly creating = signal(false);
  protected readonly reloadVersion = signal(0);

  private readonly loadList = effect(onCleanup => {
    const ready = this.ready();
    const loggedIn = this.authenticated().isAuthenticated;
    const userData: unknown = this.oidc.userData().userData;
    this.reloadVersion();
    this.subscriptions.set([]);
    this.listError.set('');
    this.loading.set(false);
    if (!ready || !loggedIn) return;
    if (!userData) {
      this.loading.set(true);
      return;
    }

    this.loading.set(true);
    const request = this.subscriptionsService.listMine().subscribe({
      next: subscriptions => {
        this.subscriptions.set(subscriptions);
        this.loading.set(false);
      },
      error: error => {
        this.listError.set(subscriptionErrorMessage(error));
        this.loading.set(false);
      },
    });
    onCleanup(() => request.unsubscribe());
  });

  protected create(): void {
    if (this.creating() || !this.ready()) return;
    this.creating.set(true);
    this.subscriptionsService.create(this.form().getRawValue()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: subscription => {
        this.creating.set(false);
        void this.router.navigate(['/calendar-subscriptions', subscription.id]);
      },
      error: error => {
        this.creating.set(false);
        this.messages.add({ severity: 'error', summary: 'Could not create subscription', detail: subscriptionErrorMessage(error) });
      },
    });
  }

  protected login(): void {
    this.navigation.login();
  }

  protected retry(): void {
    this.reloadVersion.update(value => value + 1);
  }
}
