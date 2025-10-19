import {Component, inject, OnDestroy, OnInit} from '@angular/core';
import {RouterLink, RouterLinkActive, RouterOutlet} from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import {OidcSecurityService} from 'angular-auth-oidc-client';
import {NgIf, NgOptimizedImage} from '@angular/common';
import {environment} from '../environments/environment';
import {
  NgcCookieConsentService,
  NgcInitializationErrorEvent,
  NgcInitializingEvent, NgcNoCookieLawEvent,
  NgcStatusChangeEvent
} from 'ngx-cookieconsent';
import {Subscription} from 'rxjs';
import {MatomoTracker} from 'ngx-matomo-client';
import {UserDto} from './modules/lpshows-api';
import {AuthService} from './auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NgbModule, NgIf, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'lp-calendar-web';

  private readonly authStateService = inject(AuthService);
  private readonly oidcSecurityService = inject(OidcSecurityService);
  private cookieService = inject(NgcCookieConsentService);
  private readonly tracker = inject(MatomoTracker);

  //keep refs to subscriptions to be able to unsubscribe later
  private popupOpenSubscription!: Subscription;
  private popupCloseSubscription!: Subscription;
  private initializingSubscription!: Subscription;
  private initializedSubscription!: Subscription;
  private initializationErrorSubscription!: Subscription;
  private statusChangeSubscription!: Subscription;
  private revokeChoiceSubscription!: Subscription;
  private noCookieLawSubscription!: Subscription;

  // currently logged-in user. Null if not logged in
  currentUser$: UserDto | null = null;

  isAuthenticated$ = false;

  ngOnInit(): void {
    this.initCookieConsent();

    this.authStateService.isAuthenticated$.subscribe(isAuthenticated => {
      console.debug('Authenticated:', isAuthenticated);
      this.isAuthenticated$ = isAuthenticated;

      // get current user object
      this.authStateService.userData$.subscribe(usr => {
        console.debug("User -->", usr);
        this.currentUser$ = usr;
      });

      this.authStateService.accessToken$.subscribe(at => {
        console.debug("ACCESS_TOKEN: " + at);
      });

      // Set user ID for Matomo tracker
      this.authStateService.userData$.subscribe(usr => {
        console.debug("Sending username to Matomo: ", usr);
        this.tracker.setUserId(usr?.username ?? usr?.id!);
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


  // Set theme to the user's preferred color scheme
  private updateTheme() {
    const colorMode = window.matchMedia("(prefers-color-scheme: dark)").matches ?
      "dark" :
      "light";
    document.querySelector("html")?.setAttribute("data-bs-theme", colorMode);
  }


  login(): void {
    this.oidcSecurityService.authorize();
  }

  logout(): void {
    this.oidcSecurityService.logoffLocal();
    window.location.href = environment.cognitoLogoutUrl;
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

  protected readonly environment = environment;
}
