import {
  ChangeDetectionStrategy,
  Component,
  effect,
  HostListener,
  inject,
  OnDestroy,
  OnInit,
  signal
} from '@angular/core';
import { EventType, Router, RouterOutlet } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { DateTime } from 'luxon';
import {
  NgcCookieConsentService,
  NgcInitializationErrorEvent,
  NgcInitializingEvent,
  NgcNoCookieLawEvent,
  NgcStatusChangeEvent
} from 'ngx-cookieconsent';
import { MatomoTracker } from 'ngx-matomo-client';
import { ProgressBar } from 'primeng/progressbar';
import { ScrollTop } from 'primeng/scrolltop';
import { Toast } from 'primeng/toast';
import { Subscription } from 'rxjs';
import { AuthService } from './auth/auth.service';
import { AutoBreadcrumbsComponent } from './components/v2/auto-breadcrumbs/auto-breadcrumbs.component';
import { FooterComponent } from './components/v2/footer/footer.component';
import { MainMenuComponent } from './components/v2/main-menu/main-menu.component';
import { ClockService } from './services/clock.service';
import { UsersService } from './services/users.service';
import { Dialog } from 'primeng/dialog';
import {
  CompleteUserProfileComponent
} from './components/v2/user-profile/complete-user-profile/complete-user-profile.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MainMenuComponent, FooterComponent, ScrollTop, AutoBreadcrumbsComponent, ProgressBar, Toast, Dialog, CompleteUserProfileComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'lp-calendar-web';

  readonly authStateService = inject(AuthService);
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private cookieService = inject(NgcCookieConsentService);
  private readonly tracker = inject(MatomoTracker);
  private readonly clockService = inject(ClockService);
  private readonly router = inject(Router);
  private readonly usersService = inject(UsersService);

  //keep refs to subscriptions to be able to unsubscribe later
  private popupOpenSubscription!: Subscription;
  private popupCloseSubscription!: Subscription;
  private initializingSubscription!: Subscription;
  private initializedSubscription!: Subscription;
  private initializationErrorSubscription!: Subscription;
  private statusChangeSubscription!: Subscription;
  private revokeChoiceSubscription!: Subscription;
  private noCookieLawSubscription!: Subscription;

  isAuthenticated$ = false;

  // the current clock
  currentDateTime$: DateTime = DateTime.now();

  // Loading progress of the router
  routerProgress: number = 0;
  scrolled = false;

  // display a setup screen for new users (or those that are not in the new DB yet)
  showProfileSetup$ = signal<boolean>(false);
  private checkProfileCompletedEffect = effect(() => {
    if (this.oidcSecurityService.authenticated().isAuthenticated && this.usersService.didLoadProfile() && (this.usersService.currentUser()?.username.length ?? 0) == 0) {
      console.info("User is logged in but has no completed profile yet. Showing setup screen...", this.oidcSecurityService.authenticated().isAuthenticated, this.usersService.currentUser(), this.usersService.didLoadProfile());
      this.showProfileSetup$.set(true);
    } else {
      console.debug('User either not logged in or has a completed profile. Will not show setup screen.', this.oidcSecurityService.authenticated().isAuthenticated, this.usersService.currentUser(), this.usersService.didLoadProfile());
      this.showProfileSetup$.set(false);
    }
  });

  // All relevant router events in the correct order. This can calculate the current progress
  private progressValues: EventType[] = [
    EventType.NavigationStart,
    EventType.RoutesRecognized,
    EventType.GuardsCheckStart,
    EventType.GuardsCheckEnd,
    EventType.ResolveStart,
    EventType.ResolveEnd,
    EventType.RouteConfigLoadStart,
    EventType.RouteConfigLoadEnd,
    EventType.NavigationEnd,
  ];

  // effect to update the tracker info based on the currentUser signal
  private userChangedEffect = effect(() => {
    let currentUser = this.usersService.currentUser();
    console.debug("Sending username to Matomo: ", currentUser?.username);
    this.tracker.setUserId(currentUser?.username ?? currentUser?.id!);
  });

  ngOnInit(): void {
    this.initCookieConsent();

    this.router.events.subscribe((ev) => {
      let currentIndex = this.progressValues.indexOf(ev.type);
      if (currentIndex > -1) {
        console.debug("Current index: ",currentIndex);
        this.routerProgress = currentIndex / (this.progressValues.length - 1);
      }
    });

    this.clockService.luxonClock$.subscribe(clock => {
      this.currentDateTime$ = clock;
    });

    this.authStateService.isAuthenticated$.subscribe(isAuthenticated => {
      console.debug('Authenticated:', isAuthenticated);
      this.isAuthenticated$ = isAuthenticated;

      this.authStateService.accessToken$.subscribe(at => {
        console.debug("ACCESS_TOKEN: " + at);
      });
    });

    // Manage dark/light-mode
    this.updateTheme();
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', event => {
      this.updateTheme();
    });
  }


  ngOnDestroy() {
    this.destroyCookieConsent();
  }

  @HostListener('window:scroll')
  onWindowScroll(): void {
    const scrolled = window.scrollY > 8;
    if (scrolled !== this.scrolled) {
      this.scrolled = scrolled;
    }
  }

  onProfileCompleted(): void {
    this.showProfileSetup$.set(false);
  }


  // Set theme to the user's preferred color scheme
  private updateTheme() {
    const colorMode = window.matchMedia("(prefers-color-scheme: dark)").matches ?
      "dark" :
      "light";
    document.querySelector("html")?.setAttribute("data-bs-theme", colorMode);
  }


  private initCookieConsent(): void {
    // subscribe to cookieconsent observables to react to main events
    this.popupOpenSubscription = this.cookieService.popupOpen$.subscribe(
      () => {
        // you can use this.cookieService.getConfig() to do stuff...
      });

    this.popupCloseSubscription = this.cookieService.popupClose$.subscribe(
      () => {
        // you can use this.cookieService.getConfig() to do stuff...
      });

    this.initializingSubscription = this.cookieService.initializing$.subscribe(
      (event: NgcInitializingEvent) => {
        // the cookieconsent is initilializing... Not yet safe to call methods like `NgcCookieConsentService.hasAnswered()`
        console.debug(`initializing: ${JSON.stringify(event)}`);
      });

    this.initializedSubscription = this.cookieService.initialized$.subscribe(
      () => {
        // the cookieconsent has been successfully initialized.
        // It's now safe to use methods on NgcCookieConsentService that require it, like `hasAnswered()` for eg...
        console.debug(`initialized: ${JSON.stringify(event)}`);
      });

    this.initializationErrorSubscription = this.cookieService.initializationError$.subscribe(
      (event: NgcInitializationErrorEvent) => {
        // the cookieconsent has failed to initialize...
        console.debug(`initializationError: ${JSON.stringify(event.error?.message)}`);
      });

    this.statusChangeSubscription = this.cookieService.statusChange$.subscribe(
      (event: NgcStatusChangeEvent) => {
        // you can use this.cookieService.getConfig() to do stuff...
        if (event.status != "deny" && !event.chosenBefore) {
          this.tracker.forgetUserOptOut();
        }
      });

    this.revokeChoiceSubscription = this.cookieService.revokeChoice$.subscribe(
      () => {
        // you can use this.cookieService.getConfig() to do stuff...
        this.tracker.optUserOut();
      });

    this.noCookieLawSubscription = this.cookieService.noCookieLaw$.subscribe(
      (event: NgcNoCookieLawEvent) => {
        // you can use this.cookieService.getConfig() to do stuff...
      });
  }


  private destroyCookieConsent(): void {
    // unsubscribe to cookieconsent observables to prevent memory leaks
    this.popupOpenSubscription.unsubscribe();
    this.popupCloseSubscription.unsubscribe();
    this.initializingSubscription.unsubscribe();
    this.initializedSubscription.unsubscribe();
    this.initializationErrorSubscription.unsubscribe();
    this.statusChangeSubscription.unsubscribe();
    this.revokeChoiceSubscription.unsubscribe();
    this.noCookieLawSubscription.unsubscribe();
  }

  protected readonly DateTime = DateTime;
  protected readonly EventType = EventType;
}
